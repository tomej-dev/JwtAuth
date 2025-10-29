# 🔐 JwtAuth

Aplicação **ASP.NET Core Web API** utilizando **autenticação JWT (JSON Web Token)** e **banco de dados SQLite**.  
O objetivo é demonstrar de forma prática como implementar **registro e login de usuários** protegendo rotas com **bearer tokens**.

---

## 🚀 Tecnologias utilizadas

- **.NET 9.0** (ou .NET 8.0)
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQLite**
- **JWT (System.IdentityModel.Tokens.Jwt)**
- **Swashbuckle.AspNetCore** (Swagger UI)
- **C# 12**

---

## 🧠 Conceito geral

O projeto implementa uma **API REST** onde os usuários podem:
1. Registrar-se (`/api/auth/register`);
2. Fazer login (`/api/auth/login`);
3. Receber um **token JWT** válido;
4. Acessar rotas protegidas apenas com um token válido no header HTTP (`Authorization: Bearer <token>`).

A autenticação é feita via **JWT Bearer**, configurada com uma chave secreta no `appsettings.json`.  
O token contém as *claims* (informações sobre o usuário) e expira após um tempo determinado.

---

## 🗂️ Estrutura do projeto

JwtAuth/
│
├── Controllers/
│ ├── AuthController.cs # Lida com login e registro
│ └── SampleController.cs # Exemplo de rota protegida
│
├── Data/
│ └── AppDbContext.cs # Configuração do Entity Framework e do SQLite
│
├── Models/
│ └── User.cs # Modelo de usuário
│
├── Program.cs # Configuração principal da aplicação
│
├── appsettings.json # Configurações de JWT e banco de dados
│
└── Properties/
└── launchSettings.json

yaml
Copiar código

---

## ⚙️ Como executar localmente

### 1️⃣ Clonar o repositório
```bash
git clone https://github.com/tomej-dev/JwtAuth.git
cd JwtAuth
2️⃣ Restaurar as dependências
bash
Copiar código
dotnet restore
3️⃣ Rodar a aplicação
bash
Copiar código
dotnet run
🔎 O console mostrará as URLs onde o servidor está rodando:

nginx
Copiar código
Now listening on: https://localhost:7071
Now listening on: http://localhost:5071
🧩 Testando a API
Swagger
Após rodar o projeto, acesse:

bash
Copiar código
https://localhost:7071/swagger
Lá você verá a documentação interativa gerada automaticamente pelo Swashbuckle.

🔑 Endpoints principais
Método	Rota	Descrição
POST	/api/auth/register	Registra um novo usuário
POST	/api/auth/login	Faz login e retorna o token JWT
GET	/api/sample	Rota protegida – requer Bearer Token

🛠️ Configuração do JWT
No arquivo appsettings.json, há uma chave Jwt com a configuração do token:

json
Copiar código
"Jwt": {
  "Key": "chave-super-secreta",
  "Issuer": "JwtAuthAPI",
  "Audience": "JwtAuthAPIUser",
  "ExpireMinutes": 60
}
Essa chave (Key) deve ser mantida em segredo — idealmente em variáveis de ambiente no ambiente de produção.

No Program.cs, o JWT é configurado com:

csharp
Copiar código
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            )
        };
    });
🧱 Banco de dados SQLite
O Entity Framework Core usa um banco de dados local SQLite.
No appsettings.json:

json
Copiar código
"ConnectionStrings": {
  "DefaultConnection": "Data Source=app.db"
}
Para gerar o banco:

bash
Copiar código
dotnet ef database update
Isso criará automaticamente o arquivo app.db com as tabelas definidas pelo modelo User.

🧩 Exemplo de fluxo de autenticação
Registro

json
Copiar código
POST /api/auth/register
{
    "username": "joao",
    "password": "123456"
}
Login

json
Copiar código
POST /api/auth/login
{
    "username": "joao",
    "password": "123456"
}
→ Resposta:

json
Copiar código
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6..."
}
Acesso a rota protegida

http
Copiar código
GET /api/sample
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
🧰 Swagger com autenticação JWT
No Swagger, clique em Authorize e digite o token da seguinte forma:

nginx
Copiar código
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
Assim, todas as requisições autenticadas funcionarão dentro da interface do Swagger.

🧠 Aprendizados
Implementação completa de autenticação JWT no ASP.NET Core.

Uso de SQLite com Entity Framework Core.

Criação de APIs seguras e documentadas com Swagger.

Organização limpa e modular do código.

📜 Licença
Este projeto está sob a licença MIT — sinta-se livre para usar, modificar e distribuir.

👨‍💻 Autor
João Tomé
🔗 github.com/tomej-dev
