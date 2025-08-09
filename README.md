# Azure Blob Storage Upload App (.NET 8)

Esta aplicación en **.NET 8** permite subir archivos desde una carpeta local a un contenedor en **Azure Blob Storage** usando un **SAS (Shared Access Signature)** o un **Connection String**.  
El flujo es **dinámico**: puedes subir todos los archivos o seleccionar cuáles subir, conservando los nombres originales.

---

## 1. Crear un Resource Group en Azure

```bash
az group create --name demoBicep --location eastus
```

---

## 2. Crear el Storage Account con Bicep

Archivo `storageAccount.bicep`:

```bicep
param storageAccountName string = 'stg2025demo'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

output storageAccountName string = storageAccount.name
```

Desplegarlo:

```bash
az deployment group create -g demoBicep --template-file storageAccount.bicep
```

---

## 3. Descargar la aplicación

```bash
git clone <URL-del-repositorio> && cd StorageApp
```

---

## 4. Habilitar acceso anónimo temporalmente (opcional)

En el **portal de Azure**:  
1. Ir a **Configuration**.  
2. Cambiar **Allow Blob anonymous access** a **Enabled**.  
3. Guardar.  

> **Nota:** Esto solo para pruebas, no usar en producción.

---

## 5. Obtener el Connection String

Ejecuta este comando para obtener el **Connection String** en formato `appsettings.json`:

```bash
az storage account show-connection-string --name stg2025demo --resource-group demoBicep --query "{AzureStorage:{ConnectionString:connectionString}}" -o json
```

Copia este resultado en un archivo `appsettings.json` en la raíz de tu aplicación.

---

## 6. Instalar dependencias

```bash
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Azure.Storage.Blobs
```

---

## 7. Compilar y ejecutar

```bash
dotnet clean && dotnet build && dotnet run
```

---

## 8. Configurar nivel de acceso del contenedor

En el **portal de Azure**:  
1. Ir a tu Storage Account → **Containers**.  
2. Seleccionar el contenedor.  
3. **Change access level** → **Container (anonymous read access for containers and blobs)**.  
4. Guardar.  

---

## 9. Crear un SAS (Shared Access Signature)

En el **contenedor**:  
1. **Shared access tokens** → Firmar con **Key 1**.  
2. Permisos:  
   ```
   Read (R)
   Add (A)
   Create (C)
   Write (W)
   ```
3. Definir fecha/hora de inicio y expiración.  
4. Generar y copiar el **Blob SAS URL**.

---

## 10. Ejecutar la app

La app pedirá:  
1. Nombre de la Storage Account.  
2. Blob SAS URL del contenedor.  
3. Ruta de la carpeta local.  
4. Selección de archivos (números separados por coma o Enter para todos).  

Ejemplo:

```
Nombre de la Storage Account (ej. stg2025demo): stg2025demo
Pega aquí el BLOB SAS URL: https://stg2025demo.blob.core.windows.net/democontainer?sp=racw&st=...
Ruta de la carpeta local: C:\ArchivosPrueba

Archivos encontrados:
1. foto1.png
2. reporte.pdf
3. datos.csv

Ingrese números separados por coma (Enter para subir TODOS): 1,3
Subido: foto1.png
Subido: datos.csv

Proceso completado.
```

---

## 11. Seguridad y consideraciones

- El SAS expira en la fecha/hora configurada.  
- RACW significa: **Read**, **Add**, **Create**, **Write**.  
- Revoca el SAS cuando no se use.  

---

## 12. Eliminar recursos al terminar

```bash
az group delete --name demoBicep --yes --no-wait
```

---
