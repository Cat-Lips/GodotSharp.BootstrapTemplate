using System;
using System.Collections.Generic;
using Godot;
using static Godot.SplitContainer;

namespace GodotSharp.BuildingBlocks.TestRunner
{
    [Tool, SceneTree]
    public abstract partial class TestRunner : Control
    {
        private ITest curTest;
        private int failCount;
        private int frameCount;
        private int successCount;
        private Queue<Func<ITest>> tests;

        public void Initialise(IEnumerable<Func<ITest>> tests)
            => this.tests = new(tests);

        private SplitContainer PrimaryView => _.SplitView.Get();
        private SplitContainer DetailView => _.SplitView.SplitView.Get();
        private RichTextLabel LogView => _.SplitView.LogView;
        private CenterContainer TestView => _.SplitView.SplitView.TestView;
        private RichTextLabel ErrorView => _.SplitView.SplitView.ErrorView;

        [GodotOverride]
        private void OnEnterTree()
        {
            if (Engine.EditorHint) return;
            DisplayTestHeader();
        }

        [GodotOverride]
        private void OnProcess(float _)
        {
            if (Engine.EditorHint) return;

            ++frameCount;

            if (CurrentTestComplete())
            {
                UnloadCurrentTest();
                StartNextTest();
            }

            if (AllTestsComplete())
                FinaliseTesting(quit: false);
        }

        [GodotOverride]
        private void OnUnhandledKeyInput(InputEventKey _)
        {
            if (Engine.EditorHint) return;

            if (AllTestsComplete())
                GetTree().Quit(failCount);
        }

        private bool CurrentTestComplete()
        {
            if (curTest is null)
                return true;

            if (frameCount > curTest.RequiredFrames)
            {
                DisplayTestResult("ProcessTests", curTest.EvaluateProcessTests(out var errors), errors);
                return true;
            }

            return false;
        }

        private void UnloadCurrentTest()
        {
            if (curTest is null) return;
            TestView.RemoveChild(curTest.Node);
            curTest.Node.QueueFree();
            curTest = null;
        }

        private void StartNextTest()
        {
            frameCount = 0;
            if (!HasTests()) return;
            curTest = tests.Dequeue().Invoke();

            DisplayTestResult("InitTests", curTest.RunInitTests(out var errors), errors);
            TestView.AddChild(curTest.Node, true);
            DisplayTestResult("EnterTests", curTest.RunEnterTests(out errors), errors);
        }

        private bool HasTests()
            => !(tests?.Count is 0 or null);

        private bool AllTestsComplete()
            => !HasTests() && curTest is null;

        private void FinaliseTesting(bool quit)
        {
            SetProcess(false);
            DisposeTestView();
            DisplayTestFooter();
            if (quit) GetTree().Quit(failCount);
            if (failCount is 0) DisposeErrorView();
        }

        private void DisplayTestHeader()
        {
            var header = "Running Tests...";
            GD.Print(); GD.Print(header);

            LogView.PushColor(Colors.Gray);
            LogView.AddText(header);
            LogView.Pop();

            LogView.Newline();
            LogView.PushTable(3, VAlign.Center);
        }

        private void DisplayTestResult(string test, bool? result, IEnumerable<string> errors)
        {
            switch (result)
            {
                case null:
                    DisplayTestNone(test);
                    break;
                case true:
                    DisplayTestPass(test);
                    break;
                case false:
                    DisplayTestFail(test, errors);
                    break;
            }

            void DisplayTestNone(string test)
            {
                GD.Print($"{curTest.Node.Name}.{test}: N/A");

                DisplayCell(curTest.Node.Name, Colors.Gray);
                DisplayCell(test, Colors.Gray);
                DisplayCell("N/A", Colors.Gray);
            }

            void DisplayTestPass(string test)
            {
                ++successCount;
                GD.Print($"{curTest.Node.Name}.{test}: PASS");

                DisplayCell(curTest.Node.Name);
                DisplayCell(test);
                DisplayCell("PASS", Colors.Green);
            }

            void DisplayTestFail(string test, IEnumerable<string> errors)
            {
                ++failCount;
                GD.Print($"{curTest.Node.Name}.{test}: FAIL");

                DisplayCell(curTest.Node.Name);
                DisplayCell(test);
                DisplayCell("FAIL", Colors.Red);

                ErrorView.PushColor(Colors.Red);
                ErrorView.AddText($"{curTest.Node.Name}.{test}:");
                ErrorView.Pop();

                ErrorView.Newline();
                foreach (var error in errors)
                {
                    GD.Print($" - {error}");
                    ErrorView.AddText($" - {error}");
                    ErrorView.Newline();
                }
            }

            void DisplayCell(string label, Color? colour = null)
            {
                LogView.PushCell();
                if (colour is not null) LogView.PushColor(colour.Value);
                LogView.AddText($"  {label}  ");
                if (colour is not null) LogView.Pop();
                LogView.Pop();
            }

        }

        private void DisplayTestFooter()
        {
            var footer = $"Tests Complete: {successCount} Passed, {failCount} Failed";
            GD.Print(footer); GD.Print();

            LogView.Pop(); // Table
            LogView.Newline();

            LogView.PushColor(Colors.Gray);
            LogView.AddText(footer);
            LogView.Pop();

            LogView.Newline();
            LogView.Newline();
            LogView.AddText("...Press any key to exit...");
            LogView.Newline();
        }

        private void DisposeTestView()
        {
            DetailView.DraggerVisibility = DraggerVisibilityEnum.HiddenCollapsed;
            TestView.QueueFree();
        }

        private void DisposeErrorView()
        {
            PrimaryView.DraggerVisibility = DraggerVisibilityEnum.HiddenCollapsed;
            DetailView.QueueFree();
        }
    }
}
