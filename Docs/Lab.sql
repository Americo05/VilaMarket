-- ============================================
-- NOTA: Este script contém apenas a estrutura de tabelas.
-- Configurações de servidor, credenciais e dados foram removidos por segurança.
-- ============================================

-- Tabela de histórico de migrações do Entity Framework
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
	CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
);
GO

-- Tabela de Administradores
CREATE TABLE [dbo].[Admin](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[Nome] [nvarchar](100) NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Anúncios
CREATE TABLE [dbo].[Anuncio](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdVendedor] [int] NOT NULL,
	[IdCarro] [int] NOT NULL,
	[Titulo] [nvarchar](100) NOT NULL,
	[Descricao] [nvarchar](255) NOT NULL,
	[Estado] [nvarchar](50) NULL,
	[DataCriacao] [datetime] NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Categorias
CREATE TABLE [dbo].[Categoria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [nvarchar](50) NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Carros
CREATE TABLE [dbo].[Carro](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdModelo] [int] NOT NULL,
	[IdCategoria] [int] NOT NULL,
	[IdCombustivel] [int] NOT NULL,
	[Ano] [int] NOT NULL,
	[Quilometros] [int] NOT NULL,
	[Preco] [decimal](18, 2) NOT NULL,
	[Cor] [nvarchar](50) NULL,
	[Cilindrada] [int] NULL,
	[Potencia] [int] NULL,
	[NumeroPortas] [int] NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Combustível
CREATE TABLE [dbo].[Combustivel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Tipo] [nvarchar](50) NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Compradores
CREATE TABLE [dbo].[Comprador](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Nome] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Telefone] [nvarchar](20) NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Compras
CREATE TABLE [dbo].[Compra](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdComprador] [int] NOT NULL,
	[IdAnuncio] [int] NOT NULL,
	[DataCompra] [datetime] NOT NULL,
	[ValorPago] [decimal](18, 2) NOT NULL,
	[EstadoPagamento] [nvarchar](50) NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Favoritos
CREATE TABLE [dbo].[Favorito](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdAnuncio] [int] NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[DataAdicao] [datetime] NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Imagens
CREATE TABLE [dbo].[Imagem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdAnuncio] [int] NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[Ordem] [int] NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Marcas
CREATE TABLE [dbo].[Marca](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [nvarchar](50) NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Marcas Favoritas
CREATE TABLE [dbo].[MarcaFavorita](
	[MarcaId] [int] NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	CONSTRAINT [PK_MarcaFavorita] PRIMARY KEY CLUSTERED ([MarcaId] ASC, [UserId] ASC)
);
GO

-- Tabela de Mensagens
CREATE TABLE [dbo].[Mensagem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RemetenteId] [nvarchar](450) NOT NULL,
	[DestinatarioId] [nvarchar](450) NOT NULL,
	[AnuncioId] [int] NULL,
	[Conteudo] [nvarchar](max) NOT NULL,
	[DataEnvio] [datetime] NOT NULL,
	[Lida] [bit] NOT NULL,
	CONSTRAINT [PK_Mensagem] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Modelos
CREATE TABLE [dbo].[Modelo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdMarca] [int] NOT NULL,
	[Nome] [nvarchar](50) NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Notificações
CREATE TABLE [dbo].[Notificacao](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[Titulo] [nvarchar](100) NOT NULL,
	[Mensagem] [nvarchar](max) NOT NULL,
	[DataCriacao] [datetime] NOT NULL,
	[Lida] [bit] NOT NULL,
	CONSTRAINT [PK_Notificacao] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Pesquisas
CREATE TABLE [dbo].[Pesquisa](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdComprador] [int] NOT NULL,
	[CriteriosPesquisa] [nvarchar](max) NULL,
	[DataPesquisa] [datetime] NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Reservas
CREATE TABLE [dbo].[Reserva](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdAnuncio] [int] NOT NULL,
	[IdComprador] [int] NOT NULL,
	[DataReserva] [datetime] NOT NULL,
	[DataExpiracao] [datetime] NULL,
	[Estado] [nvarchar](50) NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Vendedores
CREATE TABLE [dbo].[Vendedor](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Nome] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Telefone] [nvarchar](20) NULL,
	[Aprovado] [bit] NOT NULL,
	[DataRegisto] [datetime] NULL,
	[adminAprovadorId] [nvarchar](450) NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Tabela de Visitas
CREATE TABLE [dbo].[Visita](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdAnuncio] [int] NOT NULL,
	[IdComprador] [int] NULL,
	[DataVisita] [datetime] NOT NULL,
	[IpVisitante] [nvarchar](50) NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- ============================================
-- FOREIGN KEYS / CHAVES ESTRANGEIRAS
-- ============================================

-- Anuncio
ALTER TABLE [dbo].[Anuncio] ADD CONSTRAINT [FK_Anuncio_Vendedor] 
    FOREIGN KEY([IdVendedor]) REFERENCES [dbo].[Vendedor] ([Id]);
GO

ALTER TABLE [dbo].[Anuncio] ADD CONSTRAINT [FK_Anuncio_Carro] 
    FOREIGN KEY([IdCarro]) REFERENCES [dbo].[Carro] ([Id]);
GO

-- Carro
ALTER TABLE [dbo].[Carro] ADD CONSTRAINT [FK_Carro_Modelo] 
    FOREIGN KEY([IdModelo]) REFERENCES [dbo].[Modelo] ([Id]);
GO

ALTER TABLE [dbo].[Carro] ADD CONSTRAINT [FK_Carro_Categoria] 
    FOREIGN KEY([IdCategoria]) REFERENCES [dbo].[Categoria] ([Id]);
GO

ALTER TABLE [dbo].[Carro] ADD CONSTRAINT [FK_Carro_Combustivel] 
    FOREIGN KEY([IdCombustivel]) REFERENCES [dbo].[Combustivel] ([Id]);
GO

-- Compra
ALTER TABLE [dbo].[Compra] ADD CONSTRAINT [FK_Compra_Anuncio] 
    FOREIGN KEY([IdAnuncio]) REFERENCES [dbo].[Anuncio] ([Id]);
GO

ALTER TABLE [dbo].[Compra] ADD CONSTRAINT [FK_Compra_Comprador] 
    FOREIGN KEY([IdComprador]) REFERENCES [dbo].[Comprador] ([Id]);
GO

-- Imagem
ALTER TABLE [dbo].[Imagem] ADD CONSTRAINT [FK_Imagem_Anuncio] 
    FOREIGN KEY([IdAnuncio]) REFERENCES [dbo].[Anuncio] ([Id]) ON DELETE CASCADE;
GO

-- Modelo
ALTER TABLE [dbo].[Modelo] ADD CONSTRAINT [FK_Modelo_Marca] 
    FOREIGN KEY([IdMarca]) REFERENCES [dbo].[Marca] ([Id]);
GO

-- Pesquisa
ALTER TABLE [dbo].[Pesquisa] ADD CONSTRAINT [FK_Pesquisa_Comprador] 
    FOREIGN KEY([IdComprador]) REFERENCES [dbo].[Comprador] ([Id]);
GO

-- Reserva
ALTER TABLE [dbo].[Reserva] ADD CONSTRAINT [FK_Reserva_Anuncio] 
    FOREIGN KEY([IdAnuncio]) REFERENCES [dbo].[Anuncio] ([Id]);
GO

ALTER TABLE [dbo].[Reserva] ADD CONSTRAINT [FK_Reserva_Comprador] 
    FOREIGN KEY([IdComprador]) REFERENCES [dbo].[Comprador] ([Id]);
GO

-- Visita
ALTER TABLE [dbo].[Visita] ADD CONSTRAINT [FK_Visita_Anuncio] 
    FOREIGN KEY([IdAnuncio]) REFERENCES [dbo].[Anuncio] ([Id]);
GO

ALTER TABLE [dbo].[Visita] ADD CONSTRAINT [FK_Visita_Comprador] 
    FOREIGN KEY([IdComprador]) REFERENCES [dbo].[Comprador] ([Id]);
GO

-- ============================================
-- CHECK CONSTRAINTS / RESTRIÇÕES
-- ============================================

-- Estados permitidos para Anúncios
ALTER TABLE [dbo].[Anuncio] ADD CONSTRAINT [CK_Anuncio_EstadosPermitidos] 
    CHECK ([Estado] IN ('Ativo', 'Pausado', 'Vendido', 'Reservado', 'Reserva', 'Removido'));
GO

-- Estados de pagamento
ALTER TABLE [dbo].[Compra] ADD CONSTRAINT [CK_Compra_EstadoPagamento] 
    CHECK ([EstadoPagamento] IN ('pendente', 'pago', 'cancelado'));
GO

-- ============================================
-- NOTAS DE SEGURANÇA E IMPLEMENTAÇÃO
-- ============================================
-- 
-- IMPORTANTE - Este script NÃO inclui:
-- 1. Tabelas de autenticação ASP.NET Identity (removidas por segurança)
-- 2. Configurações específicas de servidor ou Azure SQL Database
-- 3. Dados de exemplo ou seeds
-- 4. Informações de credenciais, passwords ou tokens
-- 5. Configurações de encriptação específicas
--
-- Para ambiente de produção, adicione:
-- 1. Sistema de autenticação (ASP.NET Identity, Auth0, etc.)
-- 2. Índices adicionais para otimização de performance
-- 3. Políticas de backup e recovery
-- 4. Auditoria e logging
-- 5. Encriptação de dados sensíveis
--
-- ============================================
