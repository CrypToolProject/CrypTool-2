using CrypTool.Chaocipher.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.Chaocipher.Services
{
    public class AnimationPlayerService
    {
        private List<PresentationState> _presentationStates = new List<PresentationState>();
        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                _isPlaying = value;
                PlayingChanged?.Invoke(this, _isPlaying);
            }
        }

        public bool IsDataAvailable => _presentationStates.Any();

        public int CurrentPosition { get; private set; }

        public delegate void NeedUpdateEventHandler(object sender, PresentationState presentationState);
        public delegate void IsPlayingEventHandler(object sender, bool isPlaying);

        public event NeedUpdateEventHandler NeedUpdate;
        public event IsPlayingEventHandler PlayingChanged;

        public PresentationState GetStepToDisplay()
        {
            if (_presentationStates.Count == 0)
            {
                return null;
            }

            if (_presentationStates.Count == CurrentPosition)
            {
                IsPlaying = false;
            }
            return CurrentPosition >= _presentationStates.Count
                ? _presentationStates[_presentationStates.Count - 1]
                : _presentationStates[CurrentPosition];
        }

        public bool HasNext()
        {
            return CurrentPosition < _presentationStates.Count;
        }

        public void MoveNext()
        {
            CurrentPosition++;
        }

        public void RestartAnimation()
        {
            if (HasNext())
            {
                return;
            }

            CurrentPosition = 0;
            IsPlaying = false;
            OnNeedUpdate();
        }

        public void Play()
        {
            IsPlaying = true;
            OnNeedUpdate();
        }

        public void Pause()
        {
            IsPlaying = false;
            OnNeedUpdate();
        }

        public void JumpToStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _presentationStates.Count)
            {
                return;
            }

            CurrentPosition = stepIndex;
            OnNeedUpdate();
        }

        public void Backward()
        {
            if (CurrentPosition <= 0)
            {
                return;
            }

            CurrentPosition -= 1;
            OnNeedUpdate();
        }

        public void Forward()
        {
            if (CurrentPosition >= _presentationStates.Count)
            {
                return;
            }

            CurrentPosition += 1;
            OnNeedUpdate();
        }

        public void LoadAnimationStates(List<PresentationState> presentationStates)
        {
            _presentationStates = presentationStates;
            CurrentPosition = 0;
            OnNeedUpdate();
        }

        private void OnNeedUpdate()
        {
            if (IsDataAvailable)
            {
                NeedUpdate?.Invoke(this, GetStepToDisplay());
            }
        }

        public IEnumerable GetStepDescriptionToDisplay()
        {
            return _presentationStates
.Select(x => x.Description);
        }
    }
}