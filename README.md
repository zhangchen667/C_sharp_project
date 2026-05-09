# 我的个人空间 - 前后端分离版

基于 ASP.NET Core Web API + Vue3 + MySQL 的个人博客系统。

## 技术栈

### 后端
- ASP.NET Core Web API
- Entity Framework Core
- MySQL 数据库
- JWT 身份认证
- CORS 跨域

### 前端
- Vue 3
- Vite
- Element Plus UI 框架
- Pinia 状态管理
- Vue Router
- Axios 请求库

## 项目结构

```
C_sharp_project/
├── backend/                 # 后端 Web API
│   ├── Controllers/         # API 控制器
│   ├── Models/              # 实体模型 + DTO
│   ├── Data/                # EF Core DbContext
│   └── Program.cs           # 启动入口
│
└── frontend/                # 前端 Vue 项目
    ├── src/
    │   ├── views/           # 页面
    │   ├── router/          # 路由
    │   ├── api/             # API 请求封装
    │   ├── stores/          # 状态管理
    │   └── App.vue
    └── package.json
```

## 快速开始

### 1. 配置数据库

修改 `backend/appsettings.json` 中的数据库连接字符串：

```json
"DefaultConnection": "Server=localhost;Database=MyPersonalSpace;User=root;Password=你的密码;CharSet=utf8mb4;"
```

**连接字符串参数说明：**

| 参数 | 默认值 | 说明 |
|------|--------|------|
| Server | localhost | 数据库服务器地址 |
| Port | 3306 | MySQL 端口，不写则用默认端口 |
| Database | MyPersonalSpace | 数据库名称 |
| User | root | MySQL 用户名 |
| Password | - | MySQL 密码 |
| CharSet | utf8mb4 | 字符编码，支持中文和 Emoji |

如果你的 MySQL 端口不是默认的 `3306`，需要加上 `Port` 参数：

```json
"DefaultConnection": "Server=localhost;Port=3307;Database=MyPersonalSpace;User=root;Password=你的密码;CharSet=utf8mb4;"
```

### 2. 初始化数据库

```bash
cd backend
dotnet ef database update
```

### 3. 启动后端

```bash
cd backend
dotnet run
# 后端运行在 http://localhost:5000
```

### 4. 启动前端

```bash
cd frontend
npm install
npm run dev
# 前端运行在 http://localhost:5173
```

## 默认账号

管理员账号：
- 邮箱：`admin@example.com`
- 密码：`Admin@123`

## 功能模块

| 模块 | 功能 | 说明 |
|------|------|------|
| 用户 | 登录/注册 | JWT 认证 |
| 博文 | 列表/详情/发布/搜索/分类 | 仅管理员可发布 |
| 相册 | 列表/上传/删除 | 仅管理员可上传删除 |
| 留言 | 提交/展示/回复/删除 | 管理员可回复删除 |
| 后台 | 统计/留言管理 | 仅管理员可见 |

## API 接口

| 模块 | 接口 | 方法 | 说明 |
|------|------|------|------|
| 认证 | /api/auth/login | POST | 登录 |
| 认证 | /api/auth/register | POST | 注册 |
| 博文 | /api/posts | GET | 获取文章列表 |
| 博文 | /api/posts/{id} | GET | 获取文章详情 |
| 博文 | /api/posts | POST | 发布文章 |
| 相册 | /api/albums/photos | GET | 获取照片列表 |
| 相册 | /api/albums/upload | POST | 上传照片 |
| 留言 | /api/comments | GET | 获取留言列表 |
| 留言 | /api/comments | POST | 提交留言 |
| 留言 | /api/comments/all | GET | 获取全部留言（管理员） |
| 留言 | /api/comments/{id}/reply | PUT | 回复留言 |
| 留言 | /api/comments/{id} | DELETE | 删除留言 |

## 开发说明

### 后端开发

- 控制器位于 `Controllers/`
- 实体模型位于 `Models/`
- 数据库配置在 `Data/ApplicationDbContext`

### 前端开发

- 页面位于 `src/views/`
- API 请求在 `src/api/index.js`
- 路由在 `src/router/index.js`
- 状态管理在 `src/stores/`
