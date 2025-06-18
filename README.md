Here’s your **updated README** with all the latest changes:

---

<p align="center">
  <img src="assets/logo.png" alt="InfraScribe Logo" width="300"/>
</p>

# 🛠️ InfraScribe CLI

**CloudFormation/CDK Templates, Explained and Documented.**
Auto-generate Markdown docs, ask questions, and get summaries of your AWS infrastructure using local LLMs. Built in .NET. Runs on caffeine and existential dread.

---

## 🚀 Features

* 📚 Generate Markdown documentation grouped by resource type
* 🧠 LLM-powered summaries (via [Ollama](https://ollama.com), default: Mistral)
* ❓ Ask natural language questions about templates (`ask` command)
* 🟢 MCP server mode for API and local integrations (with `/tools`, `/run`, `/health` endpoints)
* ⚙️ Clean CLI with config/init support
* ✨ Pretty terminal output and logging

---

## 📦 Installation

```bash
git clone https://github.com/magnusseptim/infrascribe
cd infrascribe
dotnet build
```

Make sure you have `ollama` installed and serving the `mistral` model:

```bash
ollama run mistral
```

---

## ⚡ Usage

### 🏗 Generate Documentation

```bash
dotnet run -- doc ./template.yaml
```

Options:

* `--no-llm` disables LLM summaries
* `--output ./out.md` sets output file location

### 🧠 Ask Questions

```bash
dotnet run -- ask ./template.yaml "What does this VPC do?"
```

Watches you type, thinks dramatically, streams the answer letter-by-letter. Basically, sentient markdown.

---

### 🟢 MCP Server Mode (API)

Start the local MCP server:

```bash
dotnet run -- mcp
```

#### Endpoints:

* `GET /health` – Simple health check (returns `{"status":"ok"}`)
* `GET /mcp/v1/tools` – List available tools and schemas
* `POST /mcp/v1/run` – Run a tool (`doc` or `ask`) with streaming log output and structured result

**Example request (doc):**

```bash
curl -N -X POST http://localhost:5110/mcp/v1/run \
  -H "Content-Type: application/json" \
  -d '{
        "tool": "doc",
        "args": {
          "templatePath": "./Example/template.json",
          "noFiles": true
        }
      }'
```

**Example request (ask):**

```bash
curl -N -X POST http://localhost:5110/mcp/v1/run \
  -H "Content-Type: application/json" \
  -d '{
        "tool": "ask",
        "args": {
          "templatePath": "./Example/template.json",
          "question": "What does this template do?"
        }
      }'
```

**API Response:**

* Logs stream in real time.
* At the end, you'll always receive:

  ```
  -----RESULT-----
  {"Success":true,"OutputFiles":null,"Error":null,"OutputMarkdown":{"infrastructure-doc.md":"# Infrastructure Documentation ..."}}
  ```

---

## ⚙️ Config

Set up with:

```bash
dotnet run -- init
```

Creates `infrascribe.config.json`:

```json
{
  "EnableLLMSummary": true,
  "OutputDirectory": "./",
  "LogFile": "llm_usage.log"
}
```

Override via CLI flags if needed.

---

## 📁 Sample Template

Check the `Examples/` folder for example CloudFormation templates.

---

## 💬 Output Example

```
### AWS::S3::Bucket

- **AppBucket**
- **LogsBucket**

**Summary:**
These buckets store application data and access logs respectively. Versioning is enabled.
```

---

## 🤝 Contributing & Local Testing

Contributions are welcome! To contribute or run the CLI locally:

1. **Clone the repository:**

   ```bash
   git clone https://github.com/magnusseptim/infrascribe
   cd infrascribe
   ```
2. **Build and test:**

   ```bash
   dotnet build
   ```
3. **Run the CLI locally:**

   ```bash
   dotnet run -- doc ./template.yaml
   ```

Feel free to open issues or pull requests for bug fixes, new features, or improvements.

---

## 🧙‍♂️ License

InfraScribe CLI is free for personal and commercial use.
However, **attribution is required** when using the tool or its output.

You must include visible credit in your product documentation, README, or about screen.
This must include:

* The name **InfraScribe CLI**
* A link to the original repository: [https://github.com/magnusseptim/infrascribe](https://github.com/magnusseptim/infrascribe)

**Example attribution:**

> Includes components from InfraScribe CLI by \[magnusseptim] – [https://github.com/magnusseptim/infrascribe](https://github.com/magnusseptim/infrascribe)
See [LICENSE](./LICENSE) for full terms.

---