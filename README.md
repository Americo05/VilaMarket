VilaMarket - Marketplace de Veículos Usados

O VilaMarket é uma plataforma Full-Stack desenvolvida em ASP.NET Core 8.0, focada na gestão de anúncios e negociações de veículos. O projeto demonstra competências em arquiteturas MVC, integração com SQL Server e segurança de dados.

---
Nota: Este projeto foi originalmente desenhado para ambiente Cloud (Microsoft Azure). Atualmente, está configurado para execução local imediata para fins de demonstração técnica.
---

## Arquitetura Azure 
Este projeto foi integralmente desenvolvido para o ecossistema Azure, utilizando:
* Azure App Service: Hosting da aplicação Web.
* Azure SQL Database: Base de dados relacional gerida na cloud.
* Cloud Security: Configuração de firewalls e connection strings seguras.

## Funcionalidades
* Gestão de Anúncios: CRUD completo com filtros e categorias.
* Comunicação em Tempo Real: Sistema de chat entre utilizadores.
* Segurança: Autenticação e autorização via ASP.NET Core Identity.
* Backoffice: Painel administrativo para gestão de utilizadores e conteúdos.

## Stack Tecnológica
* Backend: ASP.NET Core 8.0 (MVC)
* Base de Dados: SQL Server / Azure SQL
* ORM: Entity Framework Core
* Cloud Knowledge: Preparado para deployment em Azure App Service e Azure SQL.

## 💻 Como Executar (Configuração Local)

Para correr o projeto no seu ambiente local, siga estes passos:

1. Base de Dados:
   * Certifique-se de que tem o SQL Server Express ou LocalDB instalado.
   * Na pasta `/Docs` deste repositório, encontrará o ficheiro `Lab.sql`. Execute-o no seu SQL Server para criar as tabelas e os dados iniciais.

2. Configuração:
   * No ficheiro `appsettings.json`, verifique se a `DefaultConnection` aponta para o seu servidor local.
   
3. Execução:
   * Abra o `VilaMarket.sln` no Visual Studio 2022.
   * Prima **F5** para compilar e iniciar.

## Autores
* Américo Sousa
* Pedro Fernandes

---
Desenvolvido para a unidade de LAWBD na UTAD.
