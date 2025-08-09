using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

// Helper para inferir Content-Type
static string? GetContentType(string path)
{
    var ext = Path.GetExtension(path).ToLowerInvariant();
    return ext switch
    {
        ".txt" => "text/plain",
        ".json" => "application/json",
        ".csv"  => "text/csv",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".pdf" => "application/pdf",
        ".html"=> "text/html",
        _ => null
    };
}

// 1) Pedir el nombre de la Storage Account
Console.Write("Nombre de la Storage Account (ej. stg2025demo): ");
var accountName = (Console.ReadLine() ?? string.Empty).Trim();

if (string.IsNullOrWhiteSpace(accountName))
{
    Console.WriteLine("Nombre de cuenta requerido.");
    return;
}

// Validación básica
if (!Regex.IsMatch(accountName, "^[a-z0-9]{3,24}$"))
{
    Console.WriteLine("Nombre de cuenta inválido. Usa solo minúsculas y números (3–24).");
    return;
}

Console.WriteLine($"\nBase URL: https://{accountName}.blob.core.windows.net/");

// Instrucción para crear SAS
Console.WriteLine("\n=== INSTRUCCIÓN ===");
Console.WriteLine("EN EL PORTAL O AZ CLI, CREA UN SAS DEL CONTENEDOR CON PERMISOS: R, A, C, W (RACW) Y EXPIRACIÓN CORTA.");
Console.WriteLine("COPIA EL BLOB SAS URL COMPLETO (EJEMPLO: https://{cuenta}.blob.core.windows.net/{contenedor}?sp=RACW&...&sig=...).");

Console.Write("\nPega aquí el BLOB SAS URL: ");
var containerSasUrl = Console.ReadLine();
if (string.IsNullOrWhiteSpace(containerSasUrl))
{
    Console.WriteLine("SAS URL requerido.");
    return;
}

if (!Uri.TryCreate(containerSasUrl, UriKind.Absolute, out var containerUri) ||
    !containerUri.Host.StartsWith($"{accountName}.blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("El SAS URL no corresponde a la cuenta indicada o no es válido.");
    return;
}

var container = new BlobContainerClient(containerUri);

// Ruta local
Console.Write("\nRuta de la carpeta local: ");
var localPath = Console.ReadLine();
if (string.IsNullOrWhiteSpace(localPath) || !Directory.Exists(localPath))
{
    Console.WriteLine("Ruta inválida.");
    return;
}

// Listar archivos
var files = Directory.GetFiles(localPath, "*", SearchOption.TopDirectoryOnly);
if (files.Length == 0)
{
    Console.WriteLine("No se encontraron archivos.");
    return;
}

Console.WriteLine("\nArchivos encontrados:");
for (int i = 0; i < files.Length; i++)
{
    Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
}

// Elegir si subir todos o seleccionar por números
Console.Write("\nIngrese números separados por coma (Enter para subir TODOS): ");
var selection = Console.ReadLine();

List<string> selectedFiles;
if (string.IsNullOrWhiteSpace(selection))
{
    selectedFiles = files.ToList();
}
else
{
    var parts = selection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    selectedFiles = files.Where((f, idx) =>
        parts.Contains((idx + 1).ToString())
    ).ToList();
}

if (selectedFiles.Count == 0)
{
    Console.WriteLine("No se seleccionaron archivos válidos.");
    return;
}

// Subir con nombre original
foreach (var file in selectedFiles)
{
    var blobName = Path.GetFileName(file);
    var blob = container.GetBlobClient(blobName);
    var ct = GetContentType(file);

    using var fs = File.OpenRead(file);

    var opts = new BlobUploadOptions();
    if (ct is not null) opts.HttpHeaders = new BlobHttpHeaders { ContentType = ct };

    await blob.UploadAsync(fs, opts);
    Console.WriteLine($"Subido: {blobName}");
}

Console.WriteLine("\nProceso completado.");