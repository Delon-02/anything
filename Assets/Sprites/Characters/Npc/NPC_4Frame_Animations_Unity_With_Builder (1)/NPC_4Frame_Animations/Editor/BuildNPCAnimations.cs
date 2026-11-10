using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

public static class BuildNPCAnimations
{
    private const string Root = "Assets/NPC_4Frame_Animations";
    private const string Generated = Root + "/Generated";
    private static readonly string[] Actions = { "Idle", "Walk", "Run", "Sit" };
    private static readonly string[] Directions = { "Down", "Up", "Left", "Right" };

    [MenuItem("Tools/NPC Animations/Generate Clips and Controller")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Root))
        {
            EditorUtility.DisplayDialog("NPC Animations", "Folder NPC_4Frame_Animations tidak ditemukan. Ekstrak ZIP ke dalam Assets terlebih dahulu.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(Generated)) AssetDatabase.CreateFolder(Root, "Generated");

        foreach (string action in Actions)
        foreach (string direction in Directions)
        {
            string folder = Root + "/" + action + "/" + direction;
            string[] pngs = Directory.GetFiles(folder, "*.png").OrderBy(p => p).ToArray();
            if (pngs.Length == 0) { Debug.LogWarning("Tidak ada PNG: " + folder); continue; }

            Sprite[] sprites = pngs.Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace("\\", "/"))).ToArray();
            if (sprites.Any(s => s == null))
            {
                Debug.LogError("Ada PNG yang belum diimpor sebagai Sprite: " + folder + ". Pilih PNG dan set Texture Type = Sprite (2D and UI), Sprite Mode = Single.");
                continue;
            }

            float fps = action == "Run" ? 12f : action == "Walk" ? 8f : 4f;
            AnimationClip clip = new AnimationClip { name = action + "_" + direction, frameRate = fps };
            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };

            var binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = action != "Sit";
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            string clipPath = Generated + "/" + clip.name + ".anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null) AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        AssetDatabase.SaveAssets();
        CreateController();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Berhasil", "Animation Clips dan NPC_Animator.controller sudah dibuat di folder Generated.", "OK");
    }

    private static void CreateController()
    {
        string path = Generated + "/NPC_Animator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Action", AnimatorControllerParameterType.Int);
        controller.AddParameter("Direction", AnimatorControllerParameterType.Int);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        for (int a = 0; a < Actions.Length; a++)
        for (int d = 0; d < Directions.Length; d++)
        {
            string clipPath = Generated + "/" + Actions[a] + "_" + Directions[d] + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null) continue;
            AnimatorState state = sm.AddState(Actions[a] + "_" + Directions[d]);
            state.motion = clip;
            if (a == 0 && d == 0) sm.defaultState = state;
            if (a == 0 && d == 0) continue;
            AnimatorStateTransition transition = sm.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(AnimatorConditionMode.Equals, a, "Action");
            transition.AddCondition(AnimatorConditionMode.Equals, d, "Direction");
        }
    }
}
