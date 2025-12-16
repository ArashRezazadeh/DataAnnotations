
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace DataAnnotations.Middleware;


public class XmlFormatterMiddleware(ILogger<XmlFormatterMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Check if the request accepts XML
        var acceptHeader = context.Request.Headers["Accept"].FirstOrDefault();
        var shouldConvertToXml = acceptHeader?.Contains("application/xml") == true;

        // Only intercept if XML conversion is needed
        if (shouldConvertToXml)
        {
            // Store the original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                // Create a memory stream to capture the response
                using (var responseBody = new MemoryStream())
                {
                    // Replace the response body with our memory stream
                    context.Response.Body = responseBody;

                    // Continue through the pipeline
                    await next(context);

                    // After the response is generated, check if it's a 200 OK
                    if (context.Response.StatusCode == StatusCodes.Status200OK &&
                        context.Response.ContentType?.Contains("application/json") == true)
                    {
                        logger.LogInformation("Converting JSON response to XML");

                        // Reset the memory stream to read the response
                        responseBody.Seek(0, SeekOrigin.Begin);

                        // Read the JSON response
                        var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                        
                        // Parse the JSON
                        using (var jsonDocument = JsonDocument.Parse(responseContent))
                        {
                            // Create a new memory stream for XML
                            using (var xmlStream = new MemoryStream())
                            {
                                // Write XML
                                using (var xmlWriter = XmlWriter.Create(xmlStream, new XmlWriterSettings 
                                { 
                                    Indent = true,
                                    Encoding = Encoding.UTF8
                                }))
                                {
                                    xmlWriter.WriteStartDocument();
                                    xmlWriter.WriteStartElement("Response");
                                    WriteElement(xmlWriter, jsonDocument.RootElement);
                                    xmlWriter.WriteEndElement();
                                    xmlWriter.WriteEndDocument();
                                    xmlWriter.Flush();
                                }

                                // Prepare to write XML to the original stream
                                xmlStream.Seek(0, SeekOrigin.Begin);

                                // Set response headers for XML
                                context.Response.ContentType = "application/xml; charset=utf-8";
                                context.Response.ContentLength = xmlStream.Length;

                                // Reset the original body stream
                                context.Response.Body = originalBodyStream;

                                // Copy XML to the response
                                await xmlStream.CopyToAsync(context.Response.Body);
                                return;
                            }
                        }
                    }
                    else
                    {
                        // If not converting to XML, copy the original response
                        responseBody.Seek(0, SeekOrigin.Begin);
                        context.Response.Body = originalBodyStream;
                        await responseBody.CopyToAsync(context.Response.Body);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in XML formatter middleware");
                // Restore the original stream on error
                context.Response.Body = originalBodyStream;
                throw;
            }
        }
        else
        {
            // If no XML conversion needed, just continue normally
            await next(context);
        }
    }

    private void WriteElement(XmlWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    writer.WriteStartElement(SanitizeXmlName(property.Name));
                    WriteElement(writer, property.Value);
                    writer.WriteEndElement();
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    writer.WriteStartElement("Item");
                    WriteElement(writer, item);
                    writer.WriteEndElement();
                }
                break;

            case JsonValueKind.String:
                writer.WriteString(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteValue(element.GetDecimal());
                break;

            case JsonValueKind.True:
                writer.WriteValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                break;
        }
    }

    private string SanitizeXmlName(string name)
    {
        // XML element names cannot start with numbers or contain certain characters
        if (string.IsNullOrEmpty(name))
            return "Element";
            
        // Remove invalid XML characters
        var invalidChars = new Regex(@"[^\w\d\-_]");
        var sanitized = invalidChars.Replace(name, "_");
        
        // Ensure it starts with a letter or underscore
        if (char.IsDigit(sanitized[0]))
            sanitized = "_" + sanitized;
            
        return sanitized;
    }
}
// NOTE: 

// Intercept - Capture response in MemoryStream instead of sending directly to client

// Process - Let normal request pipeline generate JSON response

// Conditional Transform - Convert JSON to XML only if client accepts XML

// Log - Record original JSON for debugging

// Write - Output either original JSON or transformed XML

// Restore - Return modified content to original HTTP response stream