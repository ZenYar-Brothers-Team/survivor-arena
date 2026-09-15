using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Content.Tests
{
    public class ContentRegistryTests
    {
        private sealed class FixtureDefinitionA : IContentDefinition
        {
            public ContentId Id { get; }

            public FixtureDefinitionA(ContentId id) => Id = id;
        }

        private sealed class FixtureDefinitionB : IContentDefinition
        {
            public ContentId Id { get; }

            public FixtureDefinitionB(ContentId id) => Id = id;
        }

        private sealed class FixtureWithReference : IContentDefinition, IReferencesContent
        {
            public ContentId Id { get; }
            public ContentRef<FixtureDefinitionA> Target { get; }

            public FixtureWithReference(ContentId id, ContentRef<FixtureDefinitionA> target)
            {
                Id = id;
                Target = target;
            }

            public IEnumerable<ContentReference> GetReferencedContent()
            {
                yield return Target.ToReference();
            }
        }

        private sealed class FixtureWithInvalidId : IContentDefinition
        {
            public ContentId Id => default;
        }

        private sealed class FixtureWithInvalidReference : IContentDefinition, IReferencesContent
        {
            public ContentId Id { get; } = "FIXTURE-REF-001";

            public IEnumerable<ContentReference> GetReferencedContent()
            {
                yield return default;
            }
        }

        private sealed class FixtureWithNullReferenceCollection : IContentDefinition, IReferencesContent
        {
            public ContentId Id { get; } = "FIXTURE-REF-001";

            public IEnumerable<ContentReference> GetReferencedContent() => null;
        }

        private sealed class FixtureBalanceDefinition : IContentDefinition
        {
            public ContentId Id { get; }
            public string DisplayName { get; }
            public float Damage { get; }

            public FixtureBalanceDefinition(ContentId id, string displayName, float damage)
            {
                Id = id;
                DisplayName = displayName;
                Damage = damage;
            }
        }

        [Test]
        public void Register_MultipleDefinitionsOfDifferentTypes_Succeeds()
        {
            var registry = new ContentRegistry();

            registry.Register(new FixtureDefinitionA("FIXTURE-A-001"));
            registry.Register(new FixtureDefinitionB("FIXTURE-B-001"));
            registry.Build();

            Assert.IsTrue(registry.TryGet<FixtureDefinitionA>("FIXTURE-A-001", out _));
            Assert.IsTrue(registry.TryGet<FixtureDefinitionB>("FIXTURE-B-001", out _));
        }

        [Test]
        public void Register_DuplicateId_Throws()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureDefinitionA("FIXTURE-A-001"));

            Assert.Throws<DuplicateContentIdException>(() =>
                registry.Register(new FixtureDefinitionB("FIXTURE-A-001")));
        }

        [Test]
        public void ContentRef_ResolvesExistingId()
        {
            var registry = new ContentRegistry();
            var target = new FixtureDefinitionA("FIXTURE-A-001");
            registry.Register(target);
            registry.Build();

            var reference = new ContentRef<FixtureDefinitionA>("FIXTURE-A-001");

            Assert.AreSame(target, reference.Resolve(registry));
        }

        [Test]
        public void Build_MissingReference_Throws()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureWithReference("FIXTURE-REF-001", new ContentRef<FixtureDefinitionA>("FIXTURE-A-999")));

            var ex = Assert.Throws<ContentValidationException>(() => registry.Build());
            StringAssert.Contains("FIXTURE-A-999", ex.Message);
        }

        [Test]
        public void Build_WrongTypeReference_Throws()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureDefinitionB("FIXTURE-B-001"));
            registry.Register(new FixtureWithReference("FIXTURE-REF-001", new ContentRef<FixtureDefinitionA>("FIXTURE-B-001")));

            var ex = Assert.Throws<ContentValidationException>(() => registry.Build());
            StringAssert.Contains("FIXTURE-B-001", ex.Message);
        }

        [Test]
        public void Build_ValidFixtureSet_DoesNotThrow()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureDefinitionA("FIXTURE-A-001"));
            registry.Register(new FixtureWithReference("FIXTURE-REF-001", new ContentRef<FixtureDefinitionA>("FIXTURE-A-001")));

            Assert.DoesNotThrow(() => registry.Build());
        }

        [Test]
        public void ContentId_Whitespace_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ContentId("   "));
        }

        [Test]
        public void Register_NullDefinition_Throws()
        {
            var registry = new ContentRegistry();

            Assert.Throws<ArgumentNullException>(() => registry.Register(null));
        }

        [Test]
        public void Register_DefaultContentId_ThrowsValidationError()
        {
            var registry = new ContentRegistry();

            var ex = Assert.Throws<ContentValidationException>(() => registry.Register(new FixtureWithInvalidId()));
            StringAssert.Contains("invalid id", ex.Message);
        }

        [Test]
        public void Build_InvalidReference_ThrowsValidationError()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureWithInvalidReference());

            var ex = Assert.Throws<ContentValidationException>(() => registry.Build());
            StringAssert.Contains("invalid content id", ex.Message);
        }

        [Test]
        public void Build_NullReferenceCollection_ThrowsValidationError()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureWithNullReferenceCollection());

            var ex = Assert.Throws<ContentValidationException>(() => registry.Build());
            StringAssert.Contains("null content reference collection", ex.Message);
        }

        [Test]
        public void Resolve_BeforeBuild_Throws()
        {
            var registry = new ContentRegistry();
            registry.Register(new FixtureDefinitionA("FIXTURE-A-001"));

            Assert.Throws<InvalidOperationException>(() => registry.Get<FixtureDefinitionA>("FIXTURE-A-001"));
        }

        [Test]
        public void BuildFrom_ConfiguredFixtureSet_LoadsValuesAndSealsRegistry()
        {
            var definition = new FixtureBalanceDefinition("FIXTURE-SKILL-001", "Fixture bolt", 12.5f);

            var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { definition });
            var loaded = registry.Get<FixtureBalanceDefinition>("FIXTURE-SKILL-001");

            Assert.IsTrue(registry.IsBuilt);
            Assert.AreSame(definition, loaded);
            Assert.AreEqual("Fixture bolt", loaded.DisplayName);
            Assert.AreEqual(12.5f, loaded.Damage);
            Assert.Throws<InvalidOperationException>(() =>
                registry.Register(new FixtureDefinitionA("FIXTURE-A-002")));
        }
    }
}
