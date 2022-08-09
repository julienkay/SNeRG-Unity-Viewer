using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using System.Text;

public static class WebRequestAsyncUtility {

    public static class WebRequestAsync<T> {

        /// <summary>
        /// Performs a WebRequest with the given URL. The response is expected to be a JSON serialized
        /// object of type <typeparamref name="T"/> and is automatically being deserialized and returned.
        /// </summary>
        /// <typeparam name="T">the type that this web request returns.</typeparam>
        public static async Task<T> SendWebRequestAsync(string url) {
            UnityWebRequest wr = UnityWebRequest.Get(url);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return JsonConvert.DeserializeObject<T>(asyncOp.webRequest.downloadHandler.text);
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }

    /// <summary>
    /// Simple WebRequest without JSON parsing. Returns plain text.
    /// </summary>
    public class WebRequestSimpleAsync {

        public static async Task<string> SendWebRequestAsync(string url) {
            UnityWebRequest wr = UnityWebRequest.Get(url);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return asyncOp.webRequest.downloadHandler.text;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }

    /// <summary>
    /// WebRequest for binary data.
    /// </summary>
    public class WebRequestBinaryAsync {

        public static async Task<byte[]> SendWebRequestAsync(string url) {
            UnityWebRequest wr = UnityWebRequest.Get(url);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return asyncOp.webRequest.downloadHandler.data;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }

    /// <summary>
    /// WebRequest for texture data.
    /// </summary>
    public class WebRequestTextureAsync {

        public static async Task<Texture2D> SendWebRequestAsync(string url) {
            UnityWebRequest wr = UnityWebRequestTexture.GetTexture(url);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return DownloadHandlerTexture.GetContent(asyncOp.webRequest);
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }
}