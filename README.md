# CoffeeNChill Canteen Management System - Part 1

## Project Overview
This project modernises the CoffeeNChill campus canteen's paper-based menu and
filing cabinet system into a cloud-enabled backend. Part 1 builds the foundation:
Azure Table Storage for the digital menu, Azure Blob Storage for staff documents,
and standalone Docker containers for local Azurite emulation and the Azure
Functions app itself.

## Important Note on Storage
The assignment brief references "Azure File Share" for staff documents. Per
lecturer guidance, Azure File Share emulation was replaced with Azure Blob
Storage (container: staff-docs) to provide equivalent document upload/list/
download functionality, as Azure File Share emulation is not reliably
supported in Azurite for this project.

## Team Members and Contributions
| Name | Student Number | Role | What they built |
|---|---|---|---|
| Kandyce Smit | ST10467645 | Menu CRUD & Documentation Lead | MenuItem model, all 5 Menu HTTP functions (Create, GetAll, GetByCategory, Update, Delete), README |
| Kristen Keve | ST10472683 | Staff Documents (Blob Storage) | UploadStaffDocument, ListStaffDocuments, DownloadStaffDocument functions |
| Thamsanqa Ncube | ST10482062 | Docker & Docker Hub | Dockerfile, Docker Hub image publishing, Azurite container setup |

## Prerequisites (Tools to Install)
- Visual Studio 2022 (with Azure development workload)
- Docker Desktop
- Postman
- Git

## Local Setup Instructions

1. Clone the repository:
git clone https://github.com/EMGPSD/cldv6212-g1-2026-poe-part1-st10467645.git

2. Create a local.settings.json file in the project root (not included by
default, must be created manually) with the following content:
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}

3. Start the Azurite storage emulator in Docker:
docker run -d --name coffeenchill-azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite azurite --skipApiVersionCheck --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0

4. Open the solution in Visual Studio and press F5 to run the Functions project locally.

## Docker Standalone Container Commands
See docs/docker-commands.md for the full reference of Docker commands used
in this project (build, push, run commands for both the Functions image and
the Azurite image).

## Docker Hub Images
- Functions image: https://hub.docker.com/r/st10482062/coffeenchill-functions
- Azurite image: https://hub.docker.com/r/st10482062/coffeenchill-azurite

## API Endpoints

### Menu (Azure Table Storage)
| Method | Route | Description |
|---|---|---|
| POST | /api/menu | Create a new menu item |
| GET | /api/menu | Get all menu items |
| GET | /api/menu/category/{category} | Get menu items filtered by category |
| PUT | /api/menu/{category}/{id} | Update a menu item's price or availability |
| DELETE | /api/menu/{category}/{id} | Delete a menu item |

### Staff Documents (Azure Blob Storage)
| Method | Route | Description |
|---|---|---|
| POST | /api/documents/upload | Upload a staff document (multipart/form-data) |
| GET | /api/documents | List all staff documents with metadata |
| GET | /api/documents/download/{fileName} | Download a specific staff document |

## Menu Management Feature (Kandyce)

The Menu Management feature replaces CoffeeNChill's paper-based chalkboard
menu with an Azure Table Storage solution. Five HTTP-triggered functions were
built to provide full CRUD operations for menu items:

* CreateMenuItem creates a new menu item and validates the input data, returning
  a 400 Bad Request when required information is missing.
* GetAllMenuItems returns all menu items stored in the table.
* GetMenuItemsByCategory filters menu items by category using the PartitionKey.
* UpdateMenuItem updates an existing menu item's price or availability and
  returns a 404 Not Found if the item does not exist.
* DeleteMenuItem deletes an existing menu item and returns a 404 Not Found if
  the item does not exist.

Each menu item uses the category as the PartitionKey and a unique SKU as the
RowKey, with additional fields for name, description, price, and availability.
Validation and existence checks were implemented to provide appropriate error
handling and prevent unhandled errors.

To test: import the Postman collection, select the "Local Azurite"
environment, open the "Menu" folder, and run the Create request first, followed
by the Get All, Get By Category, Update, and Delete requests. Error-handling
requests are also included to test validation and 404 responses.

## Staff Documents Feature (Kristen)
The staff documents feature replaces the filing cabinet of recipe sheets,
cleaning manuals and safety policies with an Azure Blob Storage container
named staff-docs. Three HTTP-triggered functions were built:
- UploadStaffDocument accepts a file via form-data, validates the
  MIME type (PDF, PNG, or JPEG only) and streams it directly into the
  container without loading the whole file into memory.
- ListStaffDocuments returns every file's name, size, and last modified date.
- DownloadStaffDocument streams the requested file back to the caller or
  returns a 404 if the file does not exist.

To test: import the Postman collection, select the "Local Azurite"
environment, open the "Documents" folder, and run Upload first (so List and
Download have a file to find), then List, then Download.

## Docker Setup (Thami)
This project runs as two separate standalone Docker containers in Part 1
(no Docker Compose yet - that comes in Part 2):
1. coffeenchill-azurite - the local Azure Storage emulator, running Azurite
   with the --skipApiVersionCheck flag (required because newer Azure SDK
   packages send a storage API version newer than the default Azurite build
   recognises).
2. coffeenchill-functions - the Azure Functions app itself, built from the
   Dockerfile in the project root using a multi-stage build (a full SDK
   image to compile, then a lightweight runtime image to actually run it).

Both images are published publicly to Docker Hub, version tagged v1.0. See
docs/docker-commands.md for the exact commands to build, push, and run both
containers, and confirm they can communicate with each other.

## Testing
A full Postman collection covering every endpoint above, with sample request
bodies and passing tests, is available in docs/CoffeeNChill_Part1_Postman_
Collection.json. Import this file along with docs/CoffeeNChill_Part1_
Environment.json into Postman, select the "Local Azurite" environment, and
run the collection.

## Video Demonstration
[YOUTUBE LINK WILL BE ADDED HERE ONCE RECORDED]

## References
References for code patterns and concepts used throughout this project are
included as comments directly above the relevant code in each file with access dates.
