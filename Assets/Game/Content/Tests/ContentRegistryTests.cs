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

        [Test]
        public void Register_MultipleDefinitionsOfDifferentTypes_Succeeds()
        {
            var registry = new ContentRegistry();

            registry.Register(new FixtureDefinitionA("FIXTURE-A-001"));
            registry.Register(new FixtureDefinitionB("FIXTURE-B-001"));

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
    }
}
