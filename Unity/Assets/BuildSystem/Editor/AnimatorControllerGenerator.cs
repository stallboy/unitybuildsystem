using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace Tools
{
    public static class AnimatorControllerGenerator
    {
        private class GenTemplate
        {
            public string prefix;
            public string template;


            public string controllerPath(string characterName)
            {
                return string.Format("{0}{1}/{2}Animator.controller", prefix, characterName, characterName);
            }

            public string templatePath()
            {
                return controllerPath(template);
            }
        }

        private static readonly List<GenTemplate> genTemplates = new List<GenTemplate>
        {
            new GenTemplate {prefix = "Assets/Hero/", template = "zhanshi_nv"},
            new GenTemplate {prefix = "Assets/Monster/", template = "Wolf"},
            new GenTemplate {prefix = "Assets/Npc/", template = "QXZ_NPC_MoFeiYan"},
            new GenTemplate {prefix = "Assets/Pet/", template = "SW_ShiTouRen_1"},
        };

        private static GenTemplate characterTemplate;
        private static string characterName;


        public static void checkAnimatorControllers()
        {
            foreach (var genTemplate in genTemplates)
            {
                characterTemplate = genTemplate;
                foreach (var directory in Directory.GetDirectories(genTemplate.prefix))
                {
                    characterName = directory.Substring(characterTemplate.prefix.Length);
                    var low = characterName.ToLower();
                    var path = characterTemplate.controllerPath(characterName);
                    if (File.Exists(path))
                    {
                        checkAnimatorController();
                    }
                    else if (low.Equals("common"))
                    {
                        //ignore
                    }
                    else
                    {
                        EditorLogger.Log("AnimatorController不存在 {0}", path);
                    }
                }
            }
        }

        public static void checkAnimatorController()
        {
            string controllername = characterTemplate.controllerPath(characterName);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllername);
            if (controller == null)
                return;


            EditorLogger.Log("----检测开始 {0}", controllername);
            initMotions();
            if (controller.layers.Length > 0)
            {
                foreach (ChildAnimatorState cstate in controller.layers[0].stateMachine.states)
                {
                    var statename = cstate.state.name;
                    if (!containsMotion(statename))
                    {
                        EditorLogger.Log("动画文件不存在 {0}", statename);
                    }
                }

                foreach (AnimatorStateTransition anyTransition in controller.layers[0].stateMachine.anyStateTransitions)
                {
                    var statename = anyTransition.destinationState.name;
                    if (anyTransition.conditions.Length == 1)
                    {
                        var param = anyTransition.conditions[0].parameter;
                        if (param != statename)
                        {
                            EditorLogger.Log("AnyState 到 {0} 的condition {1} 和目的状态名字不一致", statename, param);
                        }
                    }
                    else
                    {
                        EditorLogger.Log("AnyState 到 {0} 不止一个condition", statename);
                    }
                }
            }

            printUnusedMotions();
            EditorLogger.Log("----检测完毕 {0}", controllername);
        }


        public static void generateAnimatorControllers()
        {
            foreach (var genTemplate in genTemplates)
            {
                characterTemplate = genTemplate;

                var tmp = characterTemplate.template.ToLower();
                foreach (var directory in Directory.GetDirectories(genTemplate.prefix))
                {
                    characterName = directory.Substring(characterTemplate.prefix.Length);
                    //Debug.Log(characterName);
                    var low = characterName.ToLower();

                    if (!low.Equals(tmp) && !low.Equals("common"))
                    {
                        if (!File.Exists(characterTemplate.controllerPath(characterName)))
                        {
                            generateAnimatorController();
                        }
                    }
                }
            }
        }


        public static void generateAnimatorController()
        {
            AnimatorController originController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(characterTemplate.templatePath());
            string controllername = characterTemplate.controllerPath(characterName);
            AnimatorController newController = AnimatorController.CreateAnimatorControllerAtPath(controllername);

            EditorLogger.Log("----生成开始 {0}", controllername);
            initMotions();
            copyController(originController, newController);
            assignControllerToAnimator(newController);
            printUnusedMotions();
            EditorLogger.Log("----生成完毕 {0}", controllername);
        }


        private static readonly Dictionary<string, Motion> motions = new Dictionary<string, Motion>();
        private static readonly HashSet<string> usedmotions = new HashSet<string>();

        private static void initMotions()
        {
            motions.Clear();
            usedmotions.Clear();
            foreach (
                var file in
                    Directory.GetFiles(string.Format("{0}{1}", characterTemplate.prefix, characterName), "*.anim"))
            {
                int index = file.LastIndexOfAny(new[] {'/', '\\'});
                string motionfile = file.Substring(index + 1);
                string motionname = motionfile.Substring(0, motionfile.Length - 5);
                //EditorLogger.Log("动作 {0}", motionname);
                string animPath = string.Format("{0}{1}/{2}.anim", characterTemplate.prefix, characterName, motionname);
                Motion motion = AssetDatabase.LoadAssetAtPath<Motion>(animPath);
                motions[motionname] = motion;

                if (motionname.Equals("battlestand"))
                {
                    if (!motions.ContainsKey("idlestand"))
                    {
                        motions.Add("idlestand", motion);
                    }
                }
            }
        }

        private static bool containsMotion(string motionname)
        {
            if (motions.ContainsKey(motionname))
            {
                usedmotions.Add(motionname);
                return true;
            }
            return false;
        }

        private static void printUnusedMotions()
        {
            foreach (var kv in motions)
            {
                if (!usedmotions.Contains(kv.Key))
                {
                    EditorLogger.Log("动画文件未使用 {0}", kv.Key);
                }
            }
        }

        private static AnimatorState findState(string stateName, AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.name == stateName)
                {
                    return childState.state;
                }
            }
            return null;
        }

        private static void copyTransition(AnimatorStateTransition originTransition,
            AnimatorStateTransition newTransition)
        {
            foreach (var condition in originTransition.conditions)
            {
                newTransition.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
            newTransition.duration = originTransition.duration;
            newTransition.exitTime = originTransition.exitTime;
            newTransition.offset = originTransition.offset;
            newTransition.hasExitTime = originTransition.hasExitTime;
            newTransition.hasFixedDuration = originTransition.hasFixedDuration;
            newTransition.mute = originTransition.mute;
            newTransition.solo = originTransition.solo;
        }


        private static void copyController(AnimatorController originController, AnimatorController newController)
        {
            //parameter
            foreach (var parameter in originController.parameters)
            {
                if (parameter.type != AnimatorControllerParameterType.Trigger || containsMotion(parameter.name))
                {
                    newController.AddParameter(parameter.name, parameter.type);
                }
                else
                {
                    EditorLogger.Log("忽略参数 {0}", parameter.name);
                }
            }

            //state
            AnimatorStateMachine newStateMachine = newController.layers[0].stateMachine;
            AnimatorStateMachine originStateMachine = originController.layers[0].stateMachine;
            newStateMachine.entryPosition = originStateMachine.entryPosition;
            newStateMachine.anyStatePosition = originStateMachine.anyStatePosition;
            newStateMachine.exitPosition = originStateMachine.exitPosition;

            foreach (ChildAnimatorState cstate in originStateMachine.states)
            {
                var statename = cstate.state.name;
                if (containsMotion(statename))
                {
                    var newState = newStateMachine.AddState(statename, cstate.position);
                    newState.motion = motions[statename];
                    foreach (StateMachineBehaviour behaviour in cstate.state.behaviours)
                    {
                        newState.AddStateMachineBehaviour(behaviour.GetType());
                    }
                }
                else
                {
                    EditorLogger.Log("忽略状态 {0}", statename);
                }
            }
            newStateMachine.defaultState = findState(originStateMachine.defaultState.name, newStateMachine);


            //transition
            foreach (ChildAnimatorState originChildState in originStateMachine.states)
            {
                AnimatorState newState = findState(originChildState.state.name, newStateMachine);
                if (newState != null)
                {
                    foreach (AnimatorStateTransition originTransition in originChildState.state.transitions)
                    {
                        AnimatorState destState = findState(originTransition.destinationState.name, newStateMachine);
                        if (destState != null)
                        {
                            var newTransition = newState.AddTransition(destState);
                            copyTransition(originTransition, newTransition);
                        }
                    }
                }
            }
            foreach (AnimatorStateTransition anyTransition in originStateMachine.anyStateTransitions)
            {
                AnimatorState destState = findState(anyTransition.destinationState.name, newStateMachine);
                if (destState != null)
                {
                    var newTransition = newStateMachine.AddAnyStateTransition(destState);
                    copyTransition(anyTransition, newTransition);
                }
            }
        }


        private static void assignControllerToAnimator(AnimatorController newController)
        {
            string prefabPath = string.Format("{0}{1}.prefab", characterTemplate.prefix, characterName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                EditorLogger.Log("找不到怪物预制体: {0}", prefabPath);
                return;
            }

            Animator animator = prefab.GetComponent<Animator>();
            if (animator == null)
            {
                EditorLogger.Log("怪物预制体上没有Animator组件: {0}", prefabPath);
                return;
            }
            animator.runtimeAnimatorController = newController;
        }
    }
}