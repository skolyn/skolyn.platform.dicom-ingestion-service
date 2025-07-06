<p align="center">
  <img src="https://raw.githubusercontent.com/skolyn/.github/main/assets/skolyn_logo_dark.png" alt="Skolyn Logo" width="100"/>
</p>

# Skolyn Platform: DICOM Ingestion Service

[![CI/CD Status](https://github.com/skolyn/skolyn.platform.dicom-ingestion-service/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/skolyn/skolyn.platform.dicom-ingestion-service/actions/workflows/build-and-test.yml)
[![Code Coverage](https://img.shields.io/codecov/c/github/skolyn/skolyn.platform.dicom-ingestion-service.svg)](https://codecov.io/gh/skolyn/skolyn.platform.dicom-ingestion-service)
[![License](https://img.shields.io/badge/License-Proprietary-blue.svg)](LICENSE)
![Language](https://img.shields.io/badge/Language-C%23%20%7C%20.NET%208-blueviolet)

---

## 1. Project Overview

The **DICOM Ingestion Service** is a mission-critical, high-throughput backend microservice within the Skolyn ecosystem. It serves as the primary, secure gateway for receiving medical imaging studies from partner clinical institutions. Its core mandate is to reliably accept incoming DICOM associations (C-STORE) and DICOMweb requests (STOW-RS), persist the raw binary data to secure object storage, extract essential metadata, and enqueue the study for asynchronous processing by the downstream analysis pipeline.

This service is architected for high availability, fault tolerance, and absolute data integrity, as it represents the first point of contact for all clinical data entering our platform.

## 2. Architectural Context & Design

This service is designed as a stateless, containerized **ASP.NET Core application**, adhering to Domain-Driven Design (DDD) and Clean Architecture principles.

*   **Architectural Pattern:** RESTful API (for DICOMweb) and native DICOM SCP (for C-STORE).
*   **Primary Technologies:**
    *   **Framework:** .NET 8
    *   **DICOM Listener (C-STORE):** [fo-dicom](https://github.com/fo-dicom/fo-dicom) library.
    *   **API Framework (STOW-RS):** ASP.NET Core Minimal APIs.
    *   **Asynchronous Messaging:** Publishes messages to a **RabbitMQ** exchange upon successful ingestion.
*   **Data Flow:**
    1.  Receives a DICOM study.
    2.  Validates the study's basic structure and DICOM conformance.
    3.  Assigns a unique Skolyn Study ID (UUID).
    4.  Streams the DICOM Part 10 file directly to a secure, temporary bucket in **Azure Blob Storage / AWS S3**.
    5.  Parses essential, non-PHI DICOM tags (e.g., Modality, StudyInstanceUID).
    6.  Publishes a `StudyReceivedEvent` message to the `skolyn.events.ingestion` RabbitMQ exchange. This message contains the Study ID and the storage path, but **no PHI**.
    7.  Returns a success (200 OK) or C-STORE success response to the client.

## 3. Getting Started

### 3.1. Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
*   An IDE (Visual Studio 2022 / JetBrains Rider)

### 3.2. Local Development Setup

This service is designed to be run locally via Docker Compose for a complete, one-step setup.

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/skolyn/skolyn.platform.dicom-ingestion-service.git
    cd skolyn.platform.dicom-ingestion-service
    ```

2.  **Navigate to the development environment directory:**
    ```sh
    # This project contains the docker-compose.yml file.
    cd ./dev/
    ```

3.  **Launch the service and its dependencies:**
    ```sh
    docker-compose up --build
    ```
    This command will build the service's Docker image and launch containers for the service itself, a local RabbitMQ instance, and a local MinIO instance (to simulate S3/Blob storage).

### 3.3. Verifying the Service

The service exposes a health check endpoint. Once the containers are running, you can verify its status:

```sh
curl http://localhost:8080/health
