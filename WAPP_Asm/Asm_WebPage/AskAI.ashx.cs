using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace WAPP_Asm.Asm_WebPage
{
    public class AskAI : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/event-stream";
            context.Response.Charset = "utf-8";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BufferOutput = false;

            try { context.Response.Headers["X-Accel-Buffering"] = "no"; } catch { }

            try
            {
                if (context.Request.HttpMethod != "POST")
                {
                    SseSend(context, "error", "{\"error\":\"Only POST is supported.\"}");
                    SseDone(context);
                    return;
                }

                string body;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<Dictionary<string, object>>(body)
                           ?? new Dictionary<string, object>();

                string code = data.ContainsKey("code") ? (data["code"] ?? "").ToString() : "";
                string output = data.ContainsKey("output") ? (data["output"] ?? "").ToString() : "";
                string question = data.ContainsKey("question") ? (data["question"] ?? "").ToString() : "";

                string apiKey = ConfigurationManager.AppSettings["GroqKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    SseSend(context, "error", "{\"error\":\"Missing GroqKey in Web.config <appSettings>.\"}");
                    SseDone(context);
                    return;
                }

                string prompt =
                    "You are a helpful Python tutor. Give short hints, not full solutions.\n" +
                    "If the student has an error, explain the cause briefly and suggest 1-3 fixes.\n" +
                    "If you provide code, keep it minimal and only show the changed lines.\n\n" +
                    "Student code:\n" + code + "\n\n" +
                    "Output/Error:\n" + output + "\n\n" +
                    "Question:\n" + question;

                string url = "https://api.groq.com/openai/v1/chat/completions";
                string model = "llama-3.1-8b-instant";

                var payload = new
                {
                    model = model,
                    messages = new object[]
                    {
                        new { role = "system", content = "You are a strict but friendly Python tutor." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.4,
                    top_p = 1,
                    max_tokens = 300,
                    stream = true
                };

                string json = serializer.Serialize(payload);

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(60);

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    var req = new HttpRequestMessage(HttpMethod.Post, url);
                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    var resp = client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead)
                                     .GetAwaiter().GetResult();

                    if (!resp.IsSuccessStatusCode)
                    {
                        string err = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        string safe = JsonEscape("Groq API error (" + (int)resp.StatusCode + "): " + err);
                        SseSend(context, "error", "{\"error\":\"" + safe + "\"}");
                        SseDone(context);
                        return;
                    }

                    using (var stream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                    using (var sr = new StreamReader(stream))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (line == null) break;

                            if (line.Length == 0) continue;

                            if (line.StartsWith("data:"))
                            {
                                string dataLine = line.Substring(5).Trim();

                                if (dataLine == "[DONE]")
                                {
                                    SseDone(context);
                                    return;
                                }

                                SseSend(context, "chunk", dataLine);
                            }
                        }
                    }
                }
                SseDone(context);
            }
            catch (Exception ex)
            {
                string safe = JsonEscape("Server error: " + ex.Message);
                SseSend(context, "error", "{\"error\":\"" + safe + "\"}");
                SseDone(context);
            }
        }

        private static void SseSend(HttpContext ctx, string evt, string data)
        {
            ctx.Response.Write("event: " + evt + "\n");
            ctx.Response.Write("data: " + data + "\n\n");
            try { ctx.Response.Flush(); } catch { }
        }

        private static void SseDone(HttpContext ctx)
        {
            ctx.Response.Write("event: done\n");
            ctx.Response.Write("data: [DONE]\n\n");
            try { ctx.Response.Flush(); } catch { }
        }

        private static string JsonEscape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        public bool IsReusable { get { return false; } }
    }
}