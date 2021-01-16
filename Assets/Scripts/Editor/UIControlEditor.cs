using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Puzzled.UI
{
    [CustomEditor(typeof(UIControl), true)]
    class UIControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var control = (target as UIControl);
            var animator = (target as UIControl).GetComponent<Animator>();

            if (animator == null || animator.runtimeAnimatorController == null)
            {
                Rect buttonRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(buttonRect, "Auto Generate Animation", EditorStyles.miniButton))
                {
                    var controller = GenerateAnimatorContoller(control);
                    if (controller != null)
                    {
                        if (animator == null)
                            animator = control.gameObject.AddComponent<Animator>();

                        UnityEditor.Animations.AnimatorController.SetAnimatorController(animator, controller);
                    }
                }
            }
        }

        private static UnityEditor.Animations.AnimatorController GenerateAnimatorContoller(UIControl target)
        {
            if (target == null)
                return null;

            // Where should we create the controller?
            var path = GetSaveControllerPath(target);
            if (string.IsNullOrEmpty(path))
                return null;

            // Create controller and hook up transitions.
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach(var state in target.GetStates())
                GenerateTriggerableTransition(state, controller);

            AssetDatabase.ImportAsset(path);

            return controller;
        }

        private static string GetSaveControllerPath(UIControl target)
        {
            var defaultName = target.gameObject.name;
            var message = string.Format("Create a new animator for the game object '{0}':", defaultName);
            return EditorUtility.SaveFilePanelInProject("New Animation Contoller", defaultName, "controller", message);
        }

#if false
        private static void SetUpCurves(AnimationClip highlightedClip, AnimationClip pressedClip, string animationPath)
        {
            string[] channels = { "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z" };

            var highlightedKeys = new[] { new Keyframe(0f, 1f), new Keyframe(0.5f, 1.1f), new Keyframe(1f, 1f) };
            var highlightedCurve = new AnimationCurve(highlightedKeys);
            foreach (var channel in channels)
                AnimationUtility.SetEditorCurve(highlightedClip, EditorCurveBinding.FloatCurve(animationPath, typeof(Transform), channel), highlightedCurve);

            var pressedKeys = new[] { new Keyframe(0f, 1.15f) };
            var pressedCurve = new AnimationCurve(pressedKeys);
            foreach (var channel in channels)
                AnimationUtility.SetEditorCurve(pressedClip, EditorCurveBinding.FloatCurve(animationPath, typeof(Transform), channel), pressedCurve);
        }

        private static string BuildAnimationPath(UIControl target)
        {
            // if no target don't hook up any curves.
            var highlight = target.targetGraphic;
            if (highlight == null)
                return string.Empty;

            var startGo = highlight.gameObject;
            var toFindGo = target.gameObject;

            var pathComponents = new Stack<string>();
            while (toFindGo != startGo)
            {
                pathComponents.Push(startGo.name);

                // didn't exist in hierarchy!
                if (startGo.transform.parent == null)
                    return string.Empty;

                startGo = startGo.transform.parent.gameObject;
            }

            // calculate path
            var animPath = new StringBuilder();
            if (pathComponents.Count > 0)
                animPath.Append(pathComponents.Pop());

            while (pathComponents.Count > 0)
                animPath.Append("/").Append(pathComponents.Pop());

            return animPath.ToString();
        }
#endif

        private static AnimationClip GenerateTriggerableTransition(string name, UnityEditor.Animations.AnimatorController controller)
        {
            // Create the clip
            var clip = UnityEditor.Animations.AnimatorController.AllocateAnimatorClip(name);
            AssetDatabase.AddObjectToAsset(clip, controller);

            // Create a state in the animatior controller for this clip
            var state = controller.AddMotion(clip);

            // Add a transition property
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);

            // Add an any state transition
            var stateMachine = controller.layers[0].stateMachine;
            var transition = stateMachine.AddAnyStateTransition(state);
            transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, name);
            return clip;
        }

    }
}
