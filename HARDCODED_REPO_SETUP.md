# Profile Validation - Hardcoded Repository Setup

## ✅ **Implementation Complete**

The system now uses **hardcoded repository configuration** stored in `keys.txt`.

---

## 📝 **Configuration (`keys.txt`)**

```
OPENAI_BASE_URL=https://openai.dplit.com/v1
OPENAI_API_KEY=028fa2e1-fb69-4cca-89aa-1e11ffc4dcc1
GITHUB_REPO_OWNER=microsoft
GITHUB_REPO_NAME=vscode
```

**Update these values** to match your actual repository:
- `GITHUB_REPO_OWNER` - The repository owner (e.g., `your-company`, `your-username`)
- `GITHUB_REPO_NAME` - The repository name (e.g., `main-project`, `backend-api`)

---

## 🔌 **API Usage**

### **Simplified Request (No Repository Info Needed)**

```http
POST /api/profilevalidation/validate
Content-Type: application/json
```

**Request Body:**
```json
{
  "learnerId": 1,
  "gitHubUsername": "johndoe"
}
```

**The backend automatically:**
1. Reads `GITHUB_REPO_OWNER` from keys.txt
2. Reads `GITHUB_REPO_NAME` from keys.txt
3. Analyzes the user's contributions in that repository
4. Compares with their claimed skills

---

## 📊 **How It Works**

```
Frontend sends:
{
  "learnerId": 1,
  "gitHubUsername": "johndoe"
}
        ↓
Backend reads from keys.txt:
  GITHUB_REPO_OWNER = "microsoft"
  GITHUB_REPO_NAME = "vscode"
        ↓
Backend calls GitHub API:
  GET /repos/microsoft/vscode/commits?author=johndoe
        ↓
Analyzes commits, languages, technologies
        ↓
Compares with learner's claimed skills
        ↓
OpenAI generates validation score
        ↓
Returns comprehensive validation report
```

---

## 🎯 **Example Usage**

### **Test Request in Swagger:**

```json
{
  "learnerId": 1,
  "gitHubUsername": "octocat"
}
```

This will:
1. Get learner #1's claimed skills from database
2. Analyze GitHub user "octocat"'s contributions in `microsoft/vscode` repo
3. Return validation score and analysis

---

## 🔧 **Alternative Endpoint (GET)**

```http
GET /api/profilevalidation/validate/1/octocat
```

Same result, just using path parameters instead of request body.

---

## 📦 **What's Validated**

1. **Languages** - C#, JavaScript, Python, etc.
2. **Technologies** - ASP.NET Core, React, Docker, etc.
3. **Domain Areas** - Backend API, Database, Authentication, etc.
4. **Concepts** - Async/Await, LINQ, Dependency Injection, etc.

All extracted from actual commits in the **configured repository**.

---

## ⚙️ **To Change Repository**

1. Edit `HyperHiveBackend/keys.txt`
2. Update these lines:
   ```
   GITHUB_REPO_OWNER=your-company
   GITHUB_REPO_NAME=your-repo
   ```
3. Restart the application
4. All validations will now use the new repository

---

## ✅ **Benefits of Hardcoded Approach**

1. **Simplicity** - Frontend only needs to send username
2. **Consistency** - All learners validated against same repo
3. **Security** - Repository info not exposed to frontend
4. **Easy Updates** - Change repo in one place (keys.txt)
5. **Company-Specific** - Validate contributions to YOUR codebase

---

## 🚀 **Perfect For:**

- **Internal Company Use** - Validate employees' contributions to company repo
- **Bootcamps/Training** - Validate students' work on shared project
- **Team Projects** - Validate team members' contributions
- **Open Source Projects** - Validate contributors to specific project

---

## 📝 **Example Scenario**

**Company Setup:**
```
GITHUB_REPO_OWNER=mycompany
GITHUB_REPO_NAME=backend-api
```

**Learner Claims:**
- Skills: C#, ASP.NET Core, Entity Framework, Docker

**Validation Process:**
1. Backend analyzes learner's commits in `mycompany/backend-api`
2. Finds: C# (80%), ASP.NET Core (present), Entity Framework (present), No Docker
3. OpenAI scores: 85/100
4. Recommendation: "Add Docker configuration files to demonstrate container skills"

---

## 🎉 **Ready to Use!**

Just update the repository info in `keys.txt` and test with Swagger!

```json
POST /api/profilevalidation/validate
{
  "learnerId": 1,
  "gitHubUsername": "actual-github-username"
}
```

**The system will validate their contributions to your configured repository!** 🚀

