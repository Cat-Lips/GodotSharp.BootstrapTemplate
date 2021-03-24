using FluentAssertions;
using Godot;
using GodotSharp.BuildingBlocks.TestRunner;

namespace MyGame.Tests
{
    [SceneTree]
    public abstract partial class ExampleTestScenario : Node, ITest
    {
        public int RequiredFrames => 7;
        private int actualFrames = -1;

        [GodotOverride]
        private void OnEnterTree()
        {
            actualFrames = 0;
        }

        [GodotOverride]
        private void OnProcess(float _)
        {
            ++actualFrames;
        }

        void ITest.InitTests()
        {
            actualFrames.Should().Be(-1);
        }

        void ITest.EnterTests()
        {
            actualFrames.Should().Be(0);
        }

        void ITest.ProcessTests()
        {
            actualFrames.Should().Be(RequiredFrames);
        }
    }
}
