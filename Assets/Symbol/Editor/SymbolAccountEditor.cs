using UnityEditor;
using UnityEngine;

namespace SB
{
    public static class SymbolAccountEditor
    {
        [MenuItem( "Tools/Symbol/Account/Delete" )]
        public static void DeleteSymbolAccount()
        {
            PlayerPrefs.DeleteKey( SymbolAccountManager.FileName );
            PlayerPrefs.Save();
#if UNITY_WEBGL
            //PlayerPrefs.DeleteKey( SymbolAccountManager.FileName );
            //PlayerPrefs.Save();
#else
#endif
        }

        [MenuItem( "Tools/Symbol/Account/Set" )]
        public static void SetSymbolAccount()
        {
            PlayerPrefs.SetString( SymbolAccountManager.FileName, "" );
            PlayerPrefs.Save();
#if UNITY_WEBGL
            //PlayerPrefs.SetString( SymbolAccountManager.FileName, "" );
            //PlayerPrefs.Save();
#else
#endif
        }

    }
}

