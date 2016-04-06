﻿/**
Copyright (c) <2015>, <Devon Klompmaker>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**/
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections;

namespace uState
{
    public class TransitionListRenderer
    {
        public ReorderableList reorderableList;
        public Model model;
        public State state;

        public TransitionListRenderer(Model model, State state)
        {
            this.model = model;
            this.state = state;

            this.reorderableList = new ReorderableList(state.transitions, typeof(Transition));
            this.reorderableList.drawElementCallback += new ReorderableList.ElementCallbackDelegate(this.OnDrawElement);
            this.reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Transitions");
            };
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Transition transition = state.transitions[index];
            State transitionTarget = model.GetStateByGUID(transition.targetStateGUID);

            if (transitionTarget != null)
            {
                float x = rect.x + 5;

                GUI.Label(new Rect(x, rect.y, rect.xMax - x, EditorGUIUtility.singleLineHeight), state.name + " -> " + transitionTarget.name, EditorStyles.label);

                if (isFocused)
                {
                    StateMachineEditorWindow.selectedTransition = transition;
                }
            }
            else
            {
                state.transitions.RemoveAt(index);
            }
        }

        public void DrawList()
        {
            this.reorderableList.DoLayoutList();
        }
    }
}