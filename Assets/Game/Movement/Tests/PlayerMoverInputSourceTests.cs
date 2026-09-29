using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Game.Movement.Tests
{
    public sealed class PlayerMoverInputSourceTests
    {
        [Test]
        public void ReadMovementInput_OptionalBotSource_UsesDirectionWithoutMovingTransform()
        {
            var objectUnderTest = new GameObject("PlayerMover input seam");
            try
            {
                objectUnderTest.SetActive(false);
                var mover = objectUnderTest.AddComponent<PlayerMover>();
                var source = new FixedMovementInputSource { Direction = new Vector2(0.25f, -0.75f) };
                mover.ConfigureInputSource(source);
                var read = typeof(PlayerMover).GetMethod("ReadMovementInput", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.AreEqual(source.Direction, read.Invoke(mover, null));
                Assert.AreEqual(Vector3.zero, objectUnderTest.transform.position);
                mover.ConfigureInputSource(null);
            }
            finally { Object.DestroyImmediate(objectUnderTest); }
        }
    }
}
