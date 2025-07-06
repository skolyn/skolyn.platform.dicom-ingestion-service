<p align="center">
  <img src="[URL_to_Skolyn_Logo_High_Res.png]" alt="Skolyn Logo" width="100"/>
</p>

# Skolyn DICOM Ingestion Service

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]([Link_to_CI_Pipeline])
[![Code Coverage](https://img.shields.io/badge/coverage-95%25-brightgreen)]([Link_to_Coverage_Report])
[![License](https://img.shields.io/badge/license-Proprietary-red)](LICENSE)

---

### **1. About The Project**

`skolyn.platform.dicom-ingestion-service` is a mission-critical, high-availability backend microservice within the Skolyn ecosystem. Its sole and fundamental responsibility is to serve as the secure, unified entry point for all incoming DICOM studies from our partner clinical institutions.

This service is designed based on the principles of fault tolerance and asynchronous processing to ensure that no study is ever lost and that the core diagnostic pipeline is fed in a reliable, decoupled manner.

### **2. Architectural Design & Data Flow**

This service employs a decoupled, asynchronous architecture to maximize throughput and reliability:

1.  **Ingestion:** Receives DICOM studies via a secure API endpoint (implementing the DICOMweb STOW-RS standard).
2.  **Metadata Extraction:** A minimal set of essential DICOM tags are parsed to identify the study.
3.  **Secure Storage:** The raw DICOM object is immediately streamed to a highly durable object storage service (AWS S3).
4.  **Queueing:** A message containing the study's metadata and its storage location is published to a reliable message queue (RabbitMQ).
5.  **Acknowledgement:** A success confirmation with a unique `TraceId` is returned to the client only after the study is securely stored and queued.

This ensures that the computationally intensive pre-processing and AI analysis tasks do not block the critical ingestion process.

### **3. Getting Started**

#### **Prerequisites**

*   .NET 8 SDK
*   Docker Desktop
*   An AWS account (or localstack for local development) with an S3 bucket.
*   A running RabbitMQ instance.

#### **Local Installation & Setup**

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/skolyn/skolyn.platform.dicom-ingestion-service.git
    ```
2.  **Configure environment variables:** Create a `appsettings.Development.json` file in the `Api` project with the following structure:
    ```json
    {
      "ObjectStorage": {
        "ServiceUrl": "http://localhost:4566", // for localstack
        "BucketName": "skolyn-dicom-ingestion-dev"
      },
      "MessageQueue": {
        "HostName": "localhost",
        "ExchangeName": "dicom.studies.exchange"
      }
    }
    ```
3.  **Build and run the project:**
    ```sh
    dotnet run --project src/Skolyn.Platform.DicomIngestion.Api/
    ```
The API will be available at `https://localhost:7289`.

### **4. Usage**

The primary endpoint for DICOM ingestion is:

*   **Endpoint:** `POST /api/dicom/studies/{studyInstanceUID}`
*   **Protocol:** Implements the DICOMweb STOW-RS standard.
*   **Content-Type:** `multipart/related; type="application/dicom"`

**Example `curl` command:**
```sh
curl -X POST -H "Content-Type: application/dicom" --data-binary @/path/to/your/dicomfile.dcm http://localhost:5289/api/dicom/studies/{study-uid}```

### **5. Testing**

This project maintains a high standard of test coverage.

*   **To run unit tests:**
    ```sh
    ```
    dotnet test
    ```
```
### **6. Contributing**

Please see `CONTRIBUTING.md` for our branching strategy and code contribution guidelines.

### **7. License**

This project is proprietary and confidential. All rights are reserved by Skolyn. See `LICENSE` for more information.
