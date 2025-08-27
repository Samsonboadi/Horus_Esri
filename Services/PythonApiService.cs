﻿// Services/PythonApiService.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using SphericalImageViewer.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace SphericalImageViewer.Services
{
    public class PythonApiService
    {
        private readonly HttpClient _httpClient;
        private string _apiBaseUrl = "http://localhost:5000/api";

        public PythonApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SphericalImageViewer/1.0");
        }

        public void UpdateBaseUrl(string serverUrl)
        {
            _apiBaseUrl = $"{serverUrl.TrimEnd('/')}/api";
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
                    return result ?? new ApiResponse<T> { Success = true, Data = JsonConvert.DeserializeObject<T>(content) };
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Error = $"HTTP {response.StatusCode}: {content}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = $"Connection error: {httpEx.Message}"
                };
            }
            catch (TaskCanceledException tcEx)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = tcEx.InnerException is TimeoutException ? "Request timeout" : "Request cancelled"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", httpContent);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
                    return result ?? new ApiResponse<T> { Success = true, Data = JsonConvert.DeserializeObject<T>(content) };
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Error = $"HTTP {response.StatusCode}: {content}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = $"Connection error: {httpEx.Message}"
                };
            }
            catch (TaskCanceledException tcEx)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = tcEx.InnerException is TimeoutException ? "Request timeout" : "Request cancelled"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<byte[]>> GetImageAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    return new ApiResponse<byte[]> { Success = true, Data = imageBytes };
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Error = $"HTTP {response.StatusCode}: {content}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}


