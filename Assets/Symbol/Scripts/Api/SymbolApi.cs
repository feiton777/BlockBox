using Cysharp.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SB
{
    public static class SymbolApi
    {
        public static async UniTask<string> GetDataFromApi( string _node, string _param )
        {
            var url = $"{_node}{_param}";
            if(url.Contains( " " ) || url.Contains( "#" )) return null;

            UnityWebRequest www;

            try
            {
                Debug.Log( "url : " + url );
                www = UnityWebRequest.Get( url );
                www.SetRequestHeader( "Content-Type", "application/json" );
                await www.SendWebRequest();
            }
            catch(UnityWebRequestException e)
            {
                Debug.Log( "failed GetDataFromApi : " + url );
                return null;
            }

            switch(www.result)
            {
                case UnityWebRequest.Result.Success:
                    break;
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                default:
                    return null;
            }

            var response = www.downloadHandler.text;
            Debug.Log( "response : " + response );
            return response;
        }

        public static async UniTask<string> PostDataFromApi( string _node, string _param, object _obj )
        {
            var url = $"{_node}{_param}";
            using var client = new HttpClient();
            try
            {
                var json = JsonUtility.ToJson( _obj );
                var data = new StringContent( json, Encoding.UTF8, "application/json" );
                var response = await client.PostAsync( url, data );
                if(response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                throw new Exception( $"Error: {response.StatusCode}" );
            }
            catch(Exception ex)
            {
                throw new Exception( ex.Message );
            }
        }

        public static async UniTask<string> Announce( string _node, string payload )
        {
            var url = $"{_node}/transactions"; //var url = $"{_node}{_param}";
            if(url.Contains( " " ) || url.Contains( "#" )) return null;

            UnityWebRequest www;

            try
            {
                Debug.Log( "url : " + url );
                byte[] sendData = Encoding.UTF8.GetBytes( payload );
                www = UnityWebRequest.Put( url, sendData );
                www.SetRequestHeader( "Content-Type", "application/json" );
                await www.SendWebRequest();
            }
            catch(UnityWebRequestException e)
            {
                Debug.Log( "failed Announce : " + url );
                return null;
            }

            switch(www.result)
            {
                case UnityWebRequest.Result.Success:
                    break;
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                default:
                    return null;
            }

            var response = www.downloadHandler.text;
            Debug.Log( "response : " + response );
            return response;
        }
    }
}
