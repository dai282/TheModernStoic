# 🏛️ The Modern Stoic

> *An AI-powered journaling companion that uses RAG (Retrieval-Augmented Generation) to offer advice through the persona of Marcus Aurelius.*

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Status](https://img.shields.io/badge/status-completed-success)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#)
![Azure](https://img.shields.io/badge/Azure-0078D4?logo=microsoftazure&logoColor=white)
![React](https://img.shields.io/badge/React-20232a?logo=react&logoColor=61DAFB)

## 📖 About The Project

**The Modern Stoic** is a journaling application that goes beyond simple text storage. By leveraging **Vector Search** and **Semantic Kernel**, the application analyzes your daily entries and retrieves relevant wisdom from a curated database of Stoic texts (Meditations, Discourses, etc.).

The AI Agent (acting as Marcus Aurelius) synthesizes your specific situation with these ancient quotes to provide actionable, philosophical advice.

## 🎥 Demo

### Login & Authentication
![2025-12-2817-21-53-ezgif com-crop](https://github.com/user-attachments/assets/2ea2d6e9-2f2c-4b8c-a6f4-d83069f3ba0d)

### Adding Journal Entries
![2025-12-2817-21-53-ezgif com-crop(1)](https://github.com/user-attachments/assets/de952b2c-d865-4ada-8b88-ca462d76d32a)

### Viewing History and Deleting Entries
![2025-12-2818-02-11-ezgif com-crop](https://github.com/user-attachments/assets/72b3b063-ea6b-4d02-ae80-17fbebe4eb34)

---

## 🏗️ Architecture & Tech Stack

This project was built to explore **Azure Cloud-Native** development and **Clean Architecture** patterns.

### Backend (.NET 10)
- **Framework:** ASP.NET Core Web API (.NET 10).
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API).
- **AI Orchestration:** Microsoft Semantic Kernel (Microsoft.Extensions.AI).
- **Embedding Strategy (Cost Optimization):** Uses `all-MiniLM-L6-v2` running **locally** via ONNX Runtime. This eliminates the latency and cost of calling external embedding APIs (like OpenAI Ada).
- **Chat Generation:** Hugging Face Inference API (Llama-3.1-8B-Instruct).
- **Testing:** xUnit & NSubstitute.

### Frontend
- **Framework:** React + TypeScript + Vite.
- **Styling:** Tailwind CSS.
- **State/Auth:** Auth0 for OIDC/OAuth2 authentication.
- **Testing:** Vitest + React Testing Library.

### Cloud Infrastructure (Azure)
- **Compute:** Azure Container Apps (ACA) for the API; Azure Static Web Apps (SWA) for the Frontend.
- **Database:** Azure Cosmos DB for NoSQL (utilizing Vector Indexing/Search).
- **IaC:** Infrastructure as Code using **Azure Bicep**.
- **CI/CD:** GitHub Actions for automated build and deployment pipelines.

---

## 🧠 The AI Implementation (RAG Pattern)

1.  **Ingestion:** A custom .NET Seeder console app parses Stoic texts, chunks them, generates embeddings (Vectors) locally using ONNX, and upserts them to Cosmos DB.
2.  **Retrieval:** When a user submits a journal entry, the system embeds the entry and queries Cosmos DB for the most cosine-similar quotes.
3.  **Generation:** The system constructs a prompt containing the User's Entry + Retrieved Quotes and sends it to the LLM to generate a persona-based response.

---

## 🚀 Getting Started

### Prerequisites
*   .NET 10 SDK
*   Node.js & npm
*   Docker
*   Azure CLI

### Installation

1.  **Clone the repo**
    ```bash
    git clone https://github.com/dai282/TheModernStoic.git
    ```

2.  **Backend Setup**
    ```bash
    cd backend
    dotnet restore
    # Update appsettings.json with your CosmosDB and Auth0 credentials
    dotnet run
    ```

3.  **Frontend Setup**
    ```bash
    cd frontend
    npm install
    npm run dev
    ```

---

## 💡 Learning Outcomes

This project was my deep dive into the **Microsoft/Azure ecosystem**:
*   **Cloud Agnosticism:** Translating infrastructure concepts from AWS ECS to **Azure Container Apps**.
*   **Vector Databases:** Implementing Vector Search in a NoSQL environment (Cosmos DB).
*   **AI Abstractions:** Learning **Semantic Kernel** to standardize AI integration.
*   **Cost Optimization:** Implementing local embeddings (ONNX) to reduce API dependency and costs.
