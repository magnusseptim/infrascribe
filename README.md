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
* ⚙️ Clean CLI with config/init support
* ✨ Pretty terminal output and logging

---

## 📦 Installation

```bash
git clone https://github.com/your-user/infrascribe
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

### 🧙‍♂️ License

InfraScribe CLI is free for personal and commercial use.
However, **attribution is required** when using the tool or its output.

You must include visible credit in your product documentation, README, or about screen.
This must include:

* The name **InfraScribe CLI**
* A link to the original repository: [https://github.com/magnussptim/infrascribe](https://github.com/your-user/infrascribe)

**Example attribution:**

> This tool includes components from InfraScribe CLI by \[magnusseptim] – [https://github.com/magnusseptim/infrascribe](https://github.com/your-user/infrascribe)

See [LICENSE](./License) for full terms.
