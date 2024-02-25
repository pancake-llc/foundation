using System.Threading.Tasks;
using System;
using UnityEngine.Assertions;

namespace Pancake.DebugView
{
    public sealed class FloatingButtonPage : DefaultDebugPageBase
    {
        public delegate void ButtonClickedDelegate(Action completed);

        protected override string Title => "Submit";

        private string _buttonText;
        private ButtonClickedDelegate _onButtonClicked;
        private FloatingButton _floatingButton;
        private bool _popOnButtonClicked;

        protected override void Start()
        {
            base.Start();

            // Add padding for the floating button.
            RecyclerView.AfterPadding += 132;
        }

        public void Setup(string buttonText, Action buttonClicked, bool popOnButtonClicked = true)
        {
            Assert.IsNotNull(buttonClicked);

            _buttonText = buttonText;
            _onButtonClicked = completed =>
            {
                buttonClicked.Invoke();
                completed.Invoke();
            };
            _popOnButtonClicked = popOnButtonClicked;
        }

        public void Setup(string buttonText, ButtonClickedDelegate buttonClicked, bool popOnButtonClicked = true)
        {
            Assert.IsNotNull(buttonClicked);

            _buttonText = buttonText;
            _onButtonClicked = buttonClicked;
            _popOnButtonClicked = popOnButtonClicked;
        }

        private void DidEnter()
        {
            _floatingButton.Text = _buttonText;
            _floatingButton.OnClicked += ButtonClicked;
            _floatingButton.Show();
        }

        private void WillExit()
        {
            _floatingButton.OnClicked -= ButtonClicked;
            _floatingButton.Hide();
        }

        private void ButtonClicked()
        {
            void Completed()
            {
                _floatingButton.Interactable = true;
                if (_popOnButtonClicked)
                    DebugSheet.Of(transform).PopPage(true);
            }

            _floatingButton.Interactable = false;
            _onButtonClicked?.Invoke(Completed);
        }

        public override Task Initialize()
        {
            var debugSheet = DebugSheet.Of(transform);
            _floatingButton = debugSheet.FloatingButton;

            Reload();

            return Task.CompletedTask;
        }

        public override void DidPushEnter()
        {
            base.DidPushEnter();
            DidEnter();
        }

        public override void DidPopEnter()
        {
            base.DidPopEnter();
            DidEnter();
        }


        public override Task WillPushExit()
        {
            WillExit();
            return Task.CompletedTask;
        }

        public override Task WillPopExit()
        {
            WillExit();
            return Task.CompletedTask;
        }
    }
}