## ⚠️ Aviso de Segurança

Este ficheiro contém apenas a estrutura das tabelas do projeto. Por razões de segurança, o seguinte foi removido:

### ❌ Informação Removida (NÃO incluída neste repositório):

1. **Credenciais e Autenticação**
   - Tabelas do ASP.NET Identity (AspNetUsers, AspNetRoles, etc.)
   - Hashes de passwords
   - Tokens de autenticação
   - Configurações de OAuth/Identity providers

2. **Configurações de Servidor**
   - Connection strings
   - Configurações específicas do Azure SQL
   - Service objectives e tiers
   - Configurações de encriptação

3. **Dados Sensíveis**
   - Dados de utilizadores reais
   - Logs de auditoria
   - Seeds de dados de produção

## 📋 Estrutura da Base de Dados

### Tabelas Principais:

- **Anuncio** - Gestão de anúncios de venda
- **Carro** - Informações sobre veículos
- **Vendedor** / **Comprador** - Utilizadores do sistema
- **Compra** / **Reserva** - Transações
- **Mensagem** - Sistema de mensagens entre utilizadores
- **Favorito** - Anúncios favoritos dos utilizadores
- **Categoria** / **Marca** / **Modelo** / **Combustivel** - Dados de referência

## 🚀 Como Usar

### Setup Básico:

```sql
-- 1. Criar a base de dados
CREATE DATABASE Lab;
GO

-- 2. Usar a base de dados
USE Lab;
GO

-- 3. Executar o script
-- Execute o ficheiro Lab.sql
```

### ⚙️ Configuração Necessária para Produção:

Antes de usar em produção, você DEVE adicionar:

1. **Sistema de Autenticação**
   ```bash
   # Para ASP.NET Core Identity
   dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
   ```

2. **Configurar Connection String** (em `appsettings.json`):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": " "
     }
   }
   ```

3. **Variáveis de Ambiente** (`.env` - adicionar ao `.gitignore`):
   ```
   DB_CONNECTION_STRING=Server=...;Database=...;
   JWT_SECRET=sua_chave_secreta_aqui
   AZURE_STORAGE_KEY=...
   ```

## 🛠️ Migrações Entity Framework

Se usar EF Core Migrations:

```bash
# Criar nova migração
dotnet ef migrations add InitialCreate

# Atualizar base de dados
dotnet ef database update

# IMPORTANTE: Não incluir strings de conexão nos comandos
```

## 📚 Documentação Adicional

Para mais informações sobre segurança em aplicações .NET:
- [ASP.NET Core Security](https://docs.microsoft.com/aspnet/core/security/)
- [Azure SQL Security](https://docs.microsoft.com/azure/azure-sql/database/security-overview)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)


## 👤 Autor

Américo Sousa

---
