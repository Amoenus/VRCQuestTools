﻿using KRT.VRCQuestTools.Menus;
using UnityEditor;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Menu for avatar builder.
    /// </summary>
    internal static class AvatarBuilderMenu
    {
        [MenuItem(VRCQuestToolsMenus.MenuPaths.ShowAvatarBuilder, false, (int)VRCQuestToolsMenus.MenuPriorities.ShowAvatarBuilder)]
        private static void InitFromMenu()
        {
            EditorWindow.GetWindow<AvatarBuilderWindow>().Show();
        }
    }
}
