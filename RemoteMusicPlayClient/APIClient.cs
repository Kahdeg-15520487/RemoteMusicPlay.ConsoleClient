using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMusicPlayClient
{
    public class APIResult<T>
    {
        public readonly T Result;
        public readonly HttpStatusCode StatusCode;

        public APIResult(T result, HttpStatusCode statusCode)
        {
            Result = result;
            StatusCode = statusCode;
        }
    }

    public class APIClient : IDisposable
    {
        HttpClient client;

        public APIClient(string baseAddress = "", string token = "")
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine(client.DefaultRequestHeaders.Authorization.ToString());
            }
        }

        public APIClient(Uri baseAddress, string token = "")
        {
            client = new HttpClient
            {
                BaseAddress = baseAddress
            };
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine(client.DefaultRequestHeaders.Authorization.ToString());
            }
        }

        /// <summary>
        /// the base address of the API server
        /// </summary>
        public Uri BaseAddress
        {
            get => client.BaseAddress;
            set => client.BaseAddress = value;
        }
        /// <summary>
        /// the access token
        /// </summary>
        public string Token => client.DefaultRequestHeaders.Authorization.ToString();

        /// <summary>
        /// do a POST api request to a URI with the given object <para/>
        /// </summary>
        /// <typeparam name="T">type of request object</typeparam>
        /// <param name="uri">the api uri</param>
        /// <param name="obj">the request object</param>
        /// <returns></returns>
        public async Task<APIResult<T>> PostObjectAsync<T>(string uri, T obj)
        {
            Console.WriteLine("post {0}/{1}", BaseAddress, uri);

            HttpResponseMessage response = await client.PostAsJsonAsync(uri, obj);

            try
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsAsync<T>();
                return new APIResult<T>(result, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(response.StatusCode);
                return new APIResult<T>(default(T), response.StatusCode);
            }
        }

        /// <summary>
        /// do a POST api request to a URI with the given object <para/>
        /// use when the request object's type is different from the response object
        /// </summary>
        /// <typeparam name="T">type of response object</typeparam>
        /// <typeparam name="G">type of request object</typeparam>
        /// <param name="uri">the api uri</param>
        /// <param name="obj">the request object</param>
        /// <returns></returns>
        public async Task<APIResult<T>> PostObjectAsync<T, G>(string uri, G obj)
        {
            Console.WriteLine("post {0}/{1}", BaseAddress, uri);

            HttpResponseMessage response = await client.PostAsJsonAsync(uri, obj);

            try
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsAsync<T>();
                return new APIResult<T>(result, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(response.StatusCode);
                return new APIResult<T>(default(T), response.StatusCode);
            }
        }

        /// <summary>
        /// do a GET api request to a uri
        /// </summary>
        /// <typeparam name="T">type of the response object</typeparam>
        /// <param name="uri">the api uri</param>
        /// <returns></returns>
        public async Task<APIResult<IEnumerable<T>>> GetObjectsAsync<T>(string uri)
        {
            Console.WriteLine("get many {0}/{1}", BaseAddress, uri);

            HttpResponseMessage response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<IEnumerable<T>>();
                return new APIResult<IEnumerable<T>>(result, response.StatusCode);
            }

            return new APIResult<IEnumerable<T>>(null, response.StatusCode);
        }

        /// <summary>
        /// do a GET api request to a uri with the given id
        /// </summary>
        /// <typeparam name="T">type of the reponse object</typeparam>
        /// <param name="uri">the api uri</param>
        /// <param name="id">the request id</param>
        /// <returns></returns>
        public async Task<APIResult<T>> GetObjectAsync<T>(string uri, string id)
        {
            Console.WriteLine("get {0}/{1}/{2}", BaseAddress, uri, id);

            HttpResponseMessage response = await client.GetAsync($"{uri}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<T>();
                return new APIResult<T>(result, response.StatusCode);
            }

            return new APIResult<T>(default(T), response.StatusCode);
        }

        //public async Task<APIResult<T>> GetObjectAsync<T>(string uri)
        //{
        //    Console.WriteLine("get {0}/{1}", BaseAddress, uri);

        //    HttpResponseMessage response = await client.GetAsync($"{uri}");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var result = await response.Content.ReadAsAsync<T>();
        //        return (result, response.StatusCode);
        //    }

        //    return (default(T), response.StatusCode);
        //}

        /// <summary>
        /// do a PUT api request to a api uri with the given id and object
        /// </summary>
        /// <typeparam name="T">type of the request object</typeparam>
        /// <param name="uri">the api uri</param>
        /// <param name="id">the request id</param>
        /// <param name="obj">the put object</param>
        /// <returns></returns>
        public async Task<APIResult<Uri>> PutObjectAsync<T>(string uri, string id, T obj)
        {
            Console.WriteLine("put {0}/{1}/{2}", BaseAddress, uri, id);

            HttpResponseMessage response = await client.PutAsJsonAsync($"{uri}/{id}", obj);

            try
            {
                response.EnsureSuccessStatusCode();
                return new APIResult<Uri>(response.Headers.Location, response.StatusCode);
            }
            catch (Exception e)
            {
                return new APIResult<Uri>(null, response.StatusCode);
            }
        }

        /// <summary>
        /// do a DELETE api request to a api uri with the given id
        /// </summary>
        /// <typeparam name="T">type of the request object</typeparam>
        /// <param name="uri">the api uri</param>
        /// <param name="id">the request id</param>
        /// <returns></returns>
        public async Task<HttpStatusCode> DeleteObjectAsync<T>(string uri, string id)
        {
            Console.WriteLine("delete {0}/{1}/{2}", BaseAddress, uri, id);

            HttpResponseMessage response = await client.DeleteAsync($"{uri}/{id}");
            return response.StatusCode;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
