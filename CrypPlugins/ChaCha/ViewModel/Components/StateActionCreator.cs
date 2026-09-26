/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Class which implements the actions for state addition and little-endian step in ChaCha Hash.
    /// </summary>
    internal class StateActionCreator : ChaChaHashActionCreator
    {
        public StateActionCreator(ChaChaHashViewModel viewModel) : base(viewModel)
        {
        }

        public Action ClearStateMatrix => () =>
                                                        {
                                                            for (int i = 0; i < 16; ++i)
                                                            {
                                                                VM.StateValues[i].Value = null;
                                                                VM.StateValues[i].Mark = false;
                                                            }
                                                        };

        public Action InsertFirstOriginalState => () =>
                                                                {
                                                                    InsertOriginalState(0)();
                                                                };

        /// <summary>
        /// Reset the state matrix to the state at the start of the keystream block.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        public Action InsertOriginalState(int keystreamBlock)
        {
            return () =>
            {
                uint[] state = VM.ChaCha.OriginalState[keystreamBlock];
                for (int i = 0; i < 16; ++i)
                {
                    VM.StateValues[i].Value = state[i];
                    VM.StateValues[i].Mark = false;
                }
                if (VM.DiffusionActive)
                {
                    uint[] diffusionState = VM.ChaCha.OriginalStateDiffusion[keystreamBlock];
                    for (int i = 0; i < 16; ++i)
                    {
                        VM.DiffusionStateValues[i].Value = diffusionState[i];
                    }
                }
            };
        }

        public Action HideOriginalState => () =>
                                                         {
                                                             for (int i = 0; i < 16; ++i)
                                                             {
                                                                 VM.OriginalState[i].Value = null;
                                                             }
                                                             if (VM.DiffusionActive)
                                                             {
                                                                 for (int i = 0; i < 16; ++i)
                                                                 {
                                                                     VM.DiffusionOriginalState[i].Value = null;
                                                                 }
                                                             }
                                                         };

        public Action HideAdditionResult => () =>
                                                          {
                                                              for (int i = 0; i < 16; ++i)
                                                              {
                                                                  VM.AdditionResultState[i].Value = null;
                                                              }
                                                              if (VM.DiffusionActive)
                                                              {
                                                                  for (int i = 0; i < 16; ++i)
                                                                  {
                                                                      VM.DiffusionAdditionResultState[i].Value = null;
                                                                  }
                                                              }
                                                          };

        public Action HideLittleEndian => () =>
                                                        {
                                                            for (int i = 0; i < 16; ++i)
                                                            {
                                                                VM.LittleEndianState[i].Value = null;
                                                            }
                                                            if (VM.DiffusionActive)
                                                            {
                                                                for (int i = 0; i < 16; ++i)
                                                                {
                                                                    VM.DiffusionLittleEndianState[i].Value = null;
                                                                }
                                                            }
                                                        };

        public Action ShowOriginalState(int keystreamBlock)
        {
            return () =>
            {
                AssertKeystreamBlockInput(keystreamBlock);
                uint[] originalState = VM.ChaCha.OriginalState[keystreamBlock];
                for (int i = 0; i < 16; ++i)
                {
                    VM.OriginalState[i].Value = originalState[i];
                }
                if (VM.DiffusionActive)
                {
                    uint[] diffusionState = VM.ChaCha.OriginalStateDiffusion[keystreamBlock];
                    for (int i = 0; i < 16; ++i)
                    {
                        VM.DiffusionOriginalState[i].Value = diffusionState[i];
                    }
                }
            };
        }

        public Action ShowAdditionResult(int keystreamBlock)
        {
            return () =>
            {
                AssertKeystreamBlockInput(keystreamBlock);
                uint[] additionResult = VM.ChaCha.AdditionResultState[keystreamBlock];
                for (int i = 0; i < 16; ++i)
                {
                    VM.AdditionResultState[i].Value = additionResult[i];
                }
                if (VM.DiffusionActive)
                {
                    uint[] diffusionState = VM.ChaCha.AdditionResultStateDiffusion[keystreamBlock];
                    for (int i = 0; i < 16; ++i)
                    {
                        VM.DiffusionAdditionResultState[i].Value = diffusionState[i];
                    }
                }
            };
        }

        public Action ShowLittleEndian(int keystreamBlock)
        {
            return () =>
            {
                AssertKeystreamBlockInput(keystreamBlock);
                uint[] le = VM.ChaCha.LittleEndianState[keystreamBlock];
                for (int i = 0; i < 16; ++i)
                {
                    VM.LittleEndianState[i].Value = le[i];
                }
                if (VM.DiffusionActive)
                {
                    uint[] diffusionState = VM.ChaCha.LittleEndianStateDiffusion[keystreamBlock];
                    for (int i = 0; i < 16; ++i)
                    {
                        VM.DiffusionLittleEndianState[i].Value = diffusionState[i];
                    }
                }
            };
        }
    }
}