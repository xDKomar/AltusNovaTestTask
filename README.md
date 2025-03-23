
# AltusNova Test Project

## 1. Setup Instructions

- Clone repository  
- Go to `AltusNovaTest` folder  
- Run:

  ```bash
  dotnet run
  ```

---

## 2. Explanation of Key Design Choices

I chose the **REST** option as the most simple and common one.  
I put everything in one project (except tests) for simplicity.  
For a production-ready application, I would separate it into standard layers:

- Domain  
- Infrastructure  
- API  
- Tests

---

## 3. Proposed Hosting, Reliability, Scalability, and Security Considerations

### Hosting
I would suggest putting this application in a Docker container and using **AWS ECS Fargate** for hosting.  
It is probably the **simplest** way to host the application — but probably the most **expensive**.  
Pros and cons of other hosting options can be discussed during the interview.

### Reliability
We will set **desired running app instances > 1 per Availability Zone**, which will provide **high availability**.

### Scalability
**AWS ECS Fargate** is probably the most **scalable** way to host the application.  
It's easier to configure than an EKS cluster and doesn’t require additional maintenance.

### Security

Let’s consider 4 of the most common attack types:

- **SQL Injection**  
  We use **EF Core**, which helps prevent SQL injection attacks.

- **XSS (Cross-Site Scripting)**  
  This is an **API-only** app returning **JSON**, so it is not vulnerable to XSS.

- **CSRF (Cross-Site Request Forgery)**  
  Depends on the authentication mechanism:
  - If we use **JWT** in headers → not vulnerable.
  - If we use **cookies** → we need **HttpOnly cookies** and **anti-forgery tokens**.

- **DDOS (Distributed Denial of Service)**  
  - AWS provides basic DDOS protection by default.
  - We can also use:
    - **AWS Shield (Advanced)** for stronger protection.
    - **AWS WAF** for protection against bots and request filtering.

---

## 4. How to Run Tests

- Go to `AltusNovaTest.Tests` folder  
- Run:

  ```bash
  dotnet test
  ```
