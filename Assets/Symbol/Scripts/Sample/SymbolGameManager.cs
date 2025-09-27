using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Org.BouncyCastle.Utilities;
using SymbolSdk;
using SymbolSdk.Symbol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UnityEditor;

//using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.Rendering.DebugUI;

using SB;

public class SymbolGameManager : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text AddressText;

    [SerializeField]
    TMPro.TMP_Dropdown MosaicListDropdown;

    [SerializeField]
    TMPro.TMP_InputField DestinationAddress;

    [SerializeField]
    TMPro.TMP_Dropdown SendMosaicListDropdown;

    [SerializeField]
    TMPro.TMP_InputField SendMosaicNum;

    [SerializeField]
    GameObject HomeMenu;
    [SerializeField]
    GameObject AccountMenu;
    [SerializeField]
    GameObject MosaicMenu;
    [SerializeField]
    GameObject TransactionMenu;
    [SerializeField]
    GameObject LicenseMenu;
    [SerializeField]
    Toggle ToSToggle;
    [SerializeField]
    GameObject ToSMenu;
    [SerializeField]
    UnityEngine.UI.Button ToSStartButton;

    void Start()
    {
        var key = PlayerPrefs.GetString( "ToSAgree" );
        if(key != "")
        {
            ToSMenu.SetActive( false );
        }
    }

    void Update()
    {
        if(ToSMenu.activeSelf)
        {
            if(ToSToggle.isOn)
            {
                ToSStartButton.interactable = true;
            }
            else
            {
                ToSStartButton.interactable = false;
            }
            return;
        }

        // アカウント名更新
        if(SymbolAccountManager.Instance.AliceAddress != null && !AddressText.text.Equals( SymbolAccountManager.Instance.AliceAddress.ToString() ))
        {
            AddressText.text = SymbolAccountManager.Instance.AliceAddress.ToString();
        }
        else if(SymbolAccountManager.Instance.AliceAddress == null && !AddressText.text.Equals( "Not Found Account Info" ))
        {
            AddressText.text = "Not Found Account Info";
        }
    }

    public void Button_GenerateAccount()
    {
        SymbolAccountManager.Instance.GenerateAccount();
    }

    public void Button_DeleteAccount()
    {
        SymbolAccountManager.Instance.DeleteAccount();
    }

    public async void Button_SaveAccount()
    {
        await SaveAccountAsync();
    }

    public async UniTask<int> SaveAccountAsync()
    {
        var json = new SymbolAccountManager.AccountJson();
        json.Address = SymbolAccountManager.Instance.AliceAddress.ToString();
        json.PublicKey = SymbolAccountManager.Instance.AlicePublicKey.ToString();
        json.PrivateKey = SymbolAccountManager.Instance.LoadPrivateKey();
        string jsonStr = JsonUtility.ToJson( json );

        return await FileDialogManager.Instance.DownloadTextFileAsync( "SymbolAccount.txt", jsonStr );
    }

    public async void Button_LoadAccount()
    {
        await LoadAccountAsync();
    }

    public async UniTask<int> LoadAccountAsync()
    {
        var url = await FileDialogManager.Instance.OpenAsync();
        Debug.Log( $"{SymbolCommonManager.SymbolLogKey}{url}" );

        if(url == null || url == "") return 1;

        Debug.Log( $"{SymbolCommonManager.SymbolLogKey}{url}" );

        // WebGLでjslibからファイルを読み込む場合、短縮URLになりファイル名が取得できないので、
        // カンマ区切りでファイル名とURLを一緒にして取得するようにして、ここで分割する
        var splitUrl = url.Split( "," );
        if(1 < splitUrl.Length) url = splitUrl[ 1 ];

        string fileUrl = "file://" + url;
#if UNITY_WEBGL && !UNITY_EDITOR
        fileUrl = url;
#else
#endif

        // webgl実行時はurlが一時的な参照urlになるのでUnityWebRequestで読み込む
        using(UnityWebRequest request = UnityWebRequest.Get( fileUrl ))
        {
            var operation = request.SendWebRequest();
            await operation;

            if(request.result == UnityWebRequest.Result.Success)
            {
                string filename = Path.GetFileName( url );
                if(request.downloadHandler.text == null || request.downloadHandler.text.Length <= 0)
                {
                    Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get data." );
                    return 1;
                }

                var readText = request.downloadHandler.text;
                var json = JsonUtility.FromJson<SymbolAccountManager.AccountJson>( readText );
                PlayerPrefs.SetString( SymbolAccountManager.FileName, json.PrivateKey );
                PlayerPrefs.Save();
                SymbolAccountManager.Instance.LoadAccount();

                return 0;
            }
            else
            {
                Debug.LogError( $"{SymbolCommonManager.SymbolLogKey}An error has occurred.: {request.error}" );
                return 1;
            }
        }
    }

    public void Button_UpdateMosaicList()
    {
        UpdateMosaicListDropdown();
    }

    void UpdateMosaicListDropdown()
    {
        if(SymbolAccountManager.Instance.AliceAccountDatum != null && SymbolAccountManager.Instance.AliceAccountDatum.account != null)
        {
            MosaicListDropdown.ClearOptions();
            SendMosaicListDropdown.ClearOptions();

            for(int i = 0; i < SymbolAccountManager.Instance.AliceAccountDatum.account.mosaics.Count; i++)
            {
                var mosaic = SymbolAccountManager.Instance.AliceAccountDatum.account.mosaics[ i ];
                string addText = $"{mosaic.id} / {mosaic.amount}";
                if(MosaicListDropdown.options.Count <= i)
                {
                    MosaicListDropdown.options.Add( new TMPro.TMP_Dropdown.OptionData { text = addText } );
                }
                else
                {
                    MosaicListDropdown.options[ i ].text = addText;
                }

                if(SendMosaicListDropdown.options.Count <= i)
                {
                    SendMosaicListDropdown.options.Add( new TMPro.TMP_Dropdown.OptionData { text = addText } );
                }
                else
                {
                    SendMosaicListDropdown.options[ i ].text = addText;
                }

            }
            MosaicListDropdown.RefreshShownValue();
            SendMosaicListDropdown.RefreshShownValue();
        }
        else
        {
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get data.Please try again after a while." );
        }
    }

    public async void Button_WriteMosaicDataClick()
    {
        await SaveDialogAsync();
    }

    async UniTask<int> SaveDialogAsync()
    {
        var url = await FileDialogManager.Instance.OpenAsync();
        await SaveDataAsync( url );
        return 0;
    }

    async UniTask<int> SaveDataAsync( string url )
    {
        if(url == null || url == "")
        {
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get path." );
            return 1;
        }

        // WebGLでjslibからファイルを読み込む場合、短縮URLになりファイル名が取得できないので、
        // カンマ区切りでファイル名とURLを一緒にして取得するようにして、ここで分割する
        var splitUrl = url.Split( "," );
        if(1 < splitUrl.Length) url = splitUrl[ 1 ];

        string fileUrl = "file://" + url;
#if UNITY_WEBGL && !UNITY_EDITOR
        fileUrl = url;
#else
#endif

        using(UnityWebRequest request = UnityWebRequest.Get( fileUrl ))
        {
            var operation = request.SendWebRequest();
            await operation;

            if(request.result == UnityWebRequest.Result.Success)
            {
                string filename = Path.GetFileName( url );
                if(1 < splitUrl.Length) filename = Path.GetFileName( splitUrl[ 0 ] );

                if(request.downloadHandler.data == null || request.downloadHandler.data.Length <= 0)
                {
                    Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get data." );
                    return 1;
                }

                await SymbolMosaicManager.Instance.CreateMosaicAsync( filename, request.downloadHandler.data );
                return 0;
            }
            else
            {
                Debug.LogError( $"{SymbolCommonManager.SymbolLogKey}An error has occurred.: {request.error}" );
                return 1;
            }
        }
    }

    public async void Button_LoadData()
    {
        await LoadDialogAsync();
    }

    async UniTask<int> LoadDialogAsync()
    {
        var url = "";

#if UNITY_WEBGL && !UNITY_EDITOR
        // WEBGLは不要
#else
        url = await FileDialogManager.Instance.OpenFolderAsync();
        if(url == null || url == "") return 1;
#endif

        string mosaicListText = MosaicListDropdown.options[ MosaicListDropdown.value ].text;
        var mosaicIds = mosaicListText.Split(" / ");
        if(mosaicIds.Length <= 1)
        {
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get Mosaic ID." );
            return 1;
        }

        await LoadDataAsync( mosaicIds[0], url );
        return 0;
    }

    async UniTask<int> LoadDataAsync( string mosaicId, string url )
    {
        await SymbolMosaicManager.Instance.LoadMosaicAsync( mosaicId, url );
        return 0;
    }

    public async void Button_SendTransaction()
    {
        if(DestinationAddress.text == "")
        {
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get DestinationAddress." );
            return;
        }
        var destinationAddress = DestinationAddress.text;

        string mosaicListText = SendMosaicListDropdown.options[ SendMosaicListDropdown.value ].text;
        var mosaicIds = mosaicListText.Split( " / " );
        if(mosaicIds.Length <= 1)
        {
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get Mosaic ID." );
            return;
        }

        ulong mosaicNum = 0;
        if(SendMosaicNum.text != "")
        {
            mosaicNum = ulong.Parse( SendMosaicNum.text );
        }

        await SymbolTransactionManager.SendTransactionAsync( $"Send Test Message : {destinationAddress} : {DateTime.Now.ToString()}", destinationAddress, mosaicIds[ 0 ], mosaicNum );
    }

    public async void Button_CheckRecipientTransaction()
    {
        if(DestinationAddress.text == "")
        {
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get DestinationAddress." );
            return;
        }
        var destinationAddress = DestinationAddress.text;

        await SymbolTransactionManager.CheckRecipientTransactionAsync( destinationAddress );
    }

    public async void Button_OpenHomeMenuClick()
    {
        HomeMenu.SetActive( true );
        AccountMenu.SetActive( false );
        MosaicMenu.SetActive( false );
        TransactionMenu.SetActive( false );
        LicenseMenu.SetActive( false );
    }
    public async void Button_OpenAccountMenuClick()
    {
        HomeMenu.SetActive( false );
        AccountMenu.SetActive( true );
        MosaicMenu.SetActive( false );
        TransactionMenu.SetActive( false );
        LicenseMenu.SetActive( false );
    }
    public async void Button_OpenMosaicMenuClick()
    {
        HomeMenu.SetActive( false );
        AccountMenu.SetActive( false );
        MosaicMenu.SetActive( true );
        TransactionMenu.SetActive( false );
        LicenseMenu.SetActive( false );
    }
    public async void Button_OpenTransactionMenuClick()
    {
        HomeMenu.SetActive( false );
        AccountMenu.SetActive( false );
        MosaicMenu.SetActive( false );
        TransactionMenu.SetActive( true );
        LicenseMenu.SetActive( false );
    }
    public async void Button_OpenLicenseMenuClick()
    {
        HomeMenu.SetActive( false );
        AccountMenu.SetActive( false );
        MosaicMenu.SetActive( false );
        TransactionMenu.SetActive( false );
        LicenseMenu.SetActive( true );
    }
    public async void Button_CloseToSMenuClick()
    {
        ToSMenu.SetActive( false );
        PlayerPrefs.SetString( "ToSAgree", "true" );
        PlayerPrefs.Save();
    }

}
