using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SB
{
    public static class UnityWebRequestExtensions
    {
        public static Task<UnityWebRequest> SendWebRequestAsync( this UnityWebRequest request )
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            request.SendWebRequest().completed += _ =>
            {
                if(request.result == UnityWebRequest.Result.Success)
                {
                    tcs.SetResult( request );
                }
                else
                {
                    tcs.SetException( new System.Exception( request.error ) );
                }
            };
            return tcs.Task;
        }
    }
}
