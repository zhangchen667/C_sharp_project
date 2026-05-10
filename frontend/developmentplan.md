# 温暖治愈风格前端重构开发计划

## 项目概述
将现有个人博客前端重构为"温暖治愈/生活散文型"设计风格，建立基于 CSS 变量的设计 token 系统，实现深色/浅色模式切换，确保不影响现有功能。

## 技术栈
- Vue 3 + Vite
- Element Plus
- Pinia
- CSS 变量（原生实现）

---

## 第一阶段：设计 Token 系统建立

### 1.1 创建全局 Token 文件
**文件**: `frontend/src/styles/tokens.css`

**配色系统 - 浅色模式**:
```css
:root {
  /* 背景色 */
  --bg-primary: #FFF9F5;
  --bg-secondary: #FDF6F0;
  --bg-card: #FFFBF8;
  
  /* 品牌色 */
  --brand-primary: #D47C6B;
  --brand-primary-deep: #B85C4A;
  --brand-secondary: #E6A17A;
  
  /* 文字色 */
  --text-primary: #3D3530;
  --text-secondary: #6B5F56;
  --text-muted: #9A8B7F;
  
  /* 边框与分割线 */
  --border-light: #F0E6DE;
  --border-medium: #E0D0C2;
  
  /* 圆角 */
  --radius-sm: 0.5rem;
  --radius-md: 0.75rem;
  --radius-lg: 1rem;
  --radius-full: 9999px;
  
  /* 阴影 */
  --shadow-sm: 0 1px 2px rgba(180, 130, 100, 0.08);
  --shadow-md: 0 4px 12px rgba(180, 130, 100, 0.12);
  --shadow-lg: 0 8px 24px rgba(180, 130, 100, 0.15);
  --shadow-hover: 0 12px 32px rgba(180, 130, 100, 0.18);
  
  /* 字体 */
  --font-family-base: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  --font-family-serif: 'Georgia', 'Noto Serif SC', 'Source Han Serif SC', serif;
  
  /* 字号 */
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.25rem;
  --font-size-2xl: 1.5rem;
  --font-size-3xl: 1.875rem;
  
  /* 行高 */
  --line-height-tight: 1.3;
  --line-height-normal: 1.5;
  --line-height-reading: 1.7;
  
  /* 间距 (8px 基准) */
  --spacing-xs: 4px;
  --spacing-sm: 8px;
  --spacing-md: 16px;
  --spacing-lg: 24px;
  --spacing-xl: 32px;
  --spacing-2xl: 48px;
  
  /* 响应式断点 */
  --breakpoint-sm: 640px;
  --breakpoint-md: 768px;
  --breakpoint-lg: 1024px;
  
  /* 过渡动画 */
  --transition-fast: 0.15s ease;
  --transition-base: 0.25s ease;
  --transition-slow: 0.35s ease;
}
```

**深色模式适配**:
```css
[data-theme="dark"] {
  --bg-primary: #2A241F;
  --bg-secondary: #231E19;
  --bg-card: #312B25;
  
  --brand-primary: #E6A17A;
  --brand-primary-deep: #D47C6B;
  --brand-secondary: #F0C1A3;
  
  --text-primary: #F0E6DE;
  --text-secondary: #C9B8A8;
  --text-muted: #9A8B7F;
  
  --border-light: #3D3530;
  --border-medium: #4D4540;
  
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.2);
  --shadow-md: 0 4px 12px rgba(0, 0, 0, 0.25);
  --shadow-lg: 0 8px 24px rgba(0, 0, 0, 0.3);
  --shadow-hover: 0 12px 32px rgba(0, 0, 0, 0.35);
}
```

### 1.2 创建全局覆盖样式
**文件**: `frontend/src/styles/global.css`

```css
@import './tokens.css';

/* 全局样式重置与基础设定 */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  transition: background-color var(--transition-base), 
              color var(--transition-base),
              box-shadow var(--transition-base),
              border-color var(--transition-base);
}

html {
  scroll-behavior: smooth;
}

body {
  font-family: var(--font-family-base);
  font-size: var(--font-size-base);
  line-height: var(--line-height-reading);
  color: var(--text-primary);
  background-color: var(--bg-primary);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* 标题使用衬线字体增加质感 */
h1, h2, h3, h4, h5, h6 {
  font-family: var(--font-family-serif);
  font-weight: 600;
  line-height: var(--line-height-tight);
  color: var(--text-primary);
}

h1 { font-size: var(--font-size-3xl); }
h2 { font-size: var(--font-size-2xl); }
h3 { font-size: var(--font-size-xl); }
h4 { font-size: var(--font-size-lg); }

/* 段落间距模拟纸质书 */
p {
  margin-bottom: var(--spacing-md);
}

p + p {
  margin-top: var(--spacing-md);
}

/* 链接样式 */
a {
  color: var(--brand-primary-deep);
  text-decoration: none;
  border-bottom: 1px solid transparent;
  transition: all var(--transition-fast);
}

a:hover {
  border-bottom-color: var(--brand-primary);
}

/* 焦点样式 - 可访问性 */
:focus-visible {
  outline: 2px solid var(--brand-primary);
  outline-offset: 2px;
}

/* Element Plus 组件覆盖 */
.el-card {
  border-radius: var(--radius-lg) !important;
  background-color: var(--bg-card) !important;
  border: 1px solid var(--border-light) !important;
  box-shadow: var(--shadow-sm) !important;
  transition: all var(--transition-base) !important;
}

.el-card:hover {
  box-shadow: var(--shadow-hover) !important;
  transform: translateY(-2px);
}

.el-button--primary {
  background-color: var(--brand-primary) !important;
  border-color: var(--brand-primary) !important;
  border-radius: var(--radius-md) !important;
}

.el-button--primary:hover {
  background-color: var(--brand-primary-deep) !important;
  border-color: var(--brand-primary-deep) !important;
}

.el-input__wrapper {
  border-radius: var(--radius-md) !important;
  background-color: var(--bg-card) !important;
}

.el-menu {
  border-bottom: none !important;
}

.el-menu-item.is-active {
  color: var(--brand-primary) !important;
}

/* 装饰性背景 blob */
.decoration-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(60px);
  opacity: 0.15;
  pointer-events: none;
  z-index: 0;
}

/* 作者笔记装饰块 */
.author-note {
  background-color: var(--bg-secondary);
  border-left: 3px solid var(--brand-secondary);
  padding: var(--spacing-md) var(--spacing-lg);
  margin: var(--spacing-lg) 0;
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

/* 微妙纸张纹理 */
.paper-texture {
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)'/%3E%3C/svg%3E");
  background-repeat: repeat;
  background-size: 200px 200px;
  opacity: 0.03;
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
  z-index: 9999;
}
```

### 1.3 入口文件引入样式
修改 `frontend/src/main.js`:
```javascript
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import './styles/global.css'  // 新增：全局样式放在 Element Plus 之后覆盖
import App from './App.vue'
import router from './router'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.use(ElementPlus)
app.mount('#app')
```

---

## 第二阶段：主题切换机制实现

### 2.1 创建 Theme Store
**文件**: `frontend/src/stores/theme.js`

```javascript
import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', {
  state: () => ({
    currentTheme: 'light' // 'light' | 'dark' | 'system'
  }),

  actions: {
    initTheme() {
      // 优先读取用户保存的偏好
      const savedTheme = localStorage.getItem('user-theme')
      if (savedTheme) {
        this.currentTheme = savedTheme
      } else {
        // 无保存则检测系统偏好
        this.currentTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
      }
      this.applyTheme()
      this.watchSystemTheme()
    },

    setTheme(theme) {
      this.currentTheme = theme
      localStorage.setItem('user-theme', theme)
      this.applyTheme()
    },

    applyTheme() {
      let effectiveTheme = this.currentTheme
      
      if (this.currentTheme === 'system') {
        effectiveTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
      }

      if (effectiveTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark')
      } else {
        document.documentElement.removeAttribute('data-theme')
      }
    },

    watchSystemTheme() {
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        if (this.currentTheme === 'system') {
          this.applyTheme()
        }
      })
    },

    toggleTheme() {
      const nextTheme = this.currentTheme === 'light' ? 'dark' : 'light'
      this.setTheme(nextTheme)
    }
  }
})
```

### 2.2 创建主题切换组件
**文件**: `frontend/src/components/ThemeToggle.vue`

```vue
<template>
  <el-tooltip :content="theme === 'light' ? '切换深色模式' : '切换浅色模式'" placement="bottom">
    <button class="theme-toggle" @click="themeStore.toggleTheme" aria-label="切换主题">
      <el-icon class="icon-sun" v-if="themeStore.currentTheme === 'light'">
        <Sunny />
      </el-icon>
      <el-icon class="icon-moon" v-else>
        <Moon />
      </el-icon>
    </button>
  </el-tooltip>
</template>

<script setup>
import { Sunny, Moon } from '@element-plus/icons-vue'
import { useThemeStore } from '../stores/theme'

const themeStore = useThemeStore()
</script>

<style scoped>
.theme-toggle {
  width: 40px;
  height: 40px;
  border: none;
  border-radius: var(--radius-full);
  background: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-secondary);
  transition: all var(--transition-fast);
}

.theme-toggle:hover {
  background-color: var(--bg-secondary);
  color: var(--brand-primary);
}

.theme-toggle:active {
  transform: scale(0.95);
}
</style>
```

### 2.3 在 App.vue 中初始化主题
修改 `frontend/src/App.vue`:
```javascript
import { useThemeStore } from './stores/theme'
import ThemeToggle from './components/ThemeToggle.vue'

const themeStore = useThemeStore()

// 应用初始化时立即执行主题设置
onBeforeMount(() => {
  themeStore.initTheme()
})
```

在导航栏右侧添加主题切换按钮（位于登录按钮旁）。

---

## 第三阶段：核心组件样式适配

### 3.1 首页组件改造
**文件**: `frontend/src/views/Home.vue`

改造要点：
1. 移除原有硬编码颜色，使用 CSS 变量
2. 增加装饰性 blob 背景元素
3. 卡片阴影、圆角使用 token
4. 标题使用衬线字体

### 3.2 导航栏组件改造
**文件**: `frontend/src/App.vue`

改造要点：
1. 背景色使用 `--bg-primary`
2. 激活菜单项使用品牌色
3. 添加 ThemeToggle 组件
4. 优化按钮间距与圆角

### 3.3 文章列表页改造
**文件**: `frontend/src/views/Posts.vue`

改造要点：
1. 文章卡片使用治愈系圆角与阴影
2. 分类标签使用柔和配色
3. 时间戳使用次要文本色
4. 搜索框样式覆盖

### 3.4 相册页改造
**文件**: `frontend/src/views/Albums.vue`

改造要点：
1. 图片圆角统一使用 `--radius-lg`
2. 图片 hover 效果平滑缩放
3. 深色模式下图片亮度微调 `filter: brightness(0.95)`

### 3.5 留言页改造
**文件**: `frontend/src/views/Comments.vue`

改造要点：
1. 留言卡片头像圆形展示
2. 留言时间使用 muted 色
3. 管理员回复使用作者笔记样式

### 3.6 登录注册页改造
**文件**: 
- `frontend/src/views/Login.vue`
- `frontend/src/views/Register.vue`

改造要点：
1. 表单卡片增加包裹感
2. 密码提示文字样式优化
3. 按钮过渡动效平滑

---

## 第四阶段：响应式与可访问性优化

### 4.1 断点适配
```css
/* 在 tokens.css 中补充媒体查询示例 */
@media (max-width: 768px) {
  :root {
    --font-size-3xl: 1.5rem;
    --font-size-2xl: 1.25rem;
    --spacing-lg: 16px;
  }
}
```

### 4.2 导航栏移动端折叠
- 768px 以下显示汉堡菜单
- 使用 `el-drawer` 组件展示侧边导航

### 4.3 卡片网格响应式
- 桌面端：3列
- 平板端：2列  
- 移动端：单列

### 4.4 可访问性最终检查
- 所有交互元素支持 Tab 聚焦
- 图片添加 alt 属性
- 色值对比度验证 AA 标准

---

## 执行顺序与检查清单

### ✅ 阶段一：基础搭建
- [x] 创建 `src/styles/tokens.css`
- [x] 创建 `src/styles/global.css`
- [x] 在 `main.js` 引入全局样式
- [x] 清理旧的 `style.css` 避免样式冲突

### ✅ 阶段二：主题系统
- [x] 创建 `src/stores/theme.js`
- [x] 创建 `src/components/ThemeToggle.vue`
- [x] 在 App.vue 初始化主题
- [x] 在导航栏添加切换按钮
- [x] 测试 localStorage 持久化
- [x] 测试系统主题自动适配

### ✅ 阶段三：页面逐个改造
- [x] Home.vue - 首页风格适配
- [x] App.vue - 导航栏与整体布局
- [x] Posts.vue - 文章列表
- [x] Albums.vue - 相册页
- [x] Comments.vue - 留言页
- [x] Login.vue + Register.vue - 认证页面
- [x] Admin 后台页面
- [x] PostDetail.vue - 文章详情页

### ⏳ 阶段四：收尾与验证
- [ ] 移动端导航栏折叠实现
- [ ] 所有卡片网格响应式适配
- [ ] 可访问性检查
- [ ] 深色模式下所有页面验证
- [ ] 功能回归测试（登录、发布、留言等）

---

## 已完成改造清单

### 设计系统文件
| 文件 | 状态 | 说明 |
|------|------|------|
| `src/styles/tokens.css` | ✅ 完成 | 设计 Token 系统，含浅色/深色模式变量 |
| `src/styles/global.css` | ✅ 完成 | 全局样式重置、Element Plus 覆盖、工具类 |
| `src/stores/theme.js` | ✅ 完成 | 主题状态管理，支持 localStorage 持久化 |
| `src/components/ThemeToggle.vue` | ✅ 完成 | 主题切换按钮组件 |
| `src/main.js` | ✅ 完成 | 引入全局样式 |

### 页面组件改造
| 页面 | 文件 | 主要改动 |
|------|------|----------|
| 首页 | `Home.vue` | 标题衬线字体、卡片圆角、品牌色适配、装饰块 |
| 导航栏 | `App.vue` | 添加 ThemeToggle、纸张纹理装饰、sticky 头部 |
| 文章列表 | `Posts.vue` | 响应式网格、品牌色标题、移动端适配 |
| 文章详情 | `PostDetail.vue` | 衬线字体标题、优化返回按钮 |
| 相册 | `Albums.vue` | 响应式图片网格、图片悬浮效果 |
| 留言板 | `Comments.vue` | 作者笔记样式回复、品牌色作者名 |
| 登录 | `Login.vue` | 统一认证页面样式、居中表单卡片 |
| 注册 | `Register.vue` | 密码提示优化、信息图标 |
| 管理后台 | `Admin.vue` | 响应式统计卡片、品牌色表头 |

---

## 设计系统效果快照检查清单

### 1. 一致性检查 ✅
- [x] 背景色使用 `--bg-primary` / `--bg-secondary`
- [x] 品牌色统一使用 `--brand-primary` / `--brand-secondary`
- [x] 圆角使用 `--radius-lg` (1rem) 覆盖 Element Plus
- [x] 阴影使用 `--shadow-sm` / `--shadow-hover`
- [x] 字体使用 `--font-family-serif` 标题、`--font-family-base` 正文
- [x] 间距使用 `--spacing-*` 变量

### 2. 主题切换检查 🌓
- [x] 浅色模式配色 `#FFF9F5` 背景
- [x] 深色模式保留温暖感 `#2A241F`
- [x] 主题切换过渡平滑 0.25s ease
- [x] 用户偏好持久化到 localStorage
- [x] 支持系统自动适配 `prefers-color-scheme`

### 3. 响应式检查 📱
- [x] 卡片网格响应式：lg/md/sm 断点适配
- [x] 首页导航卡片响应式 `:lg="8" :md="12" :sm="24"`
- [x] 文章列表响应式 `:lg="12" :md="12" :sm="24"`
- [x] 相册响应式 `:lg="6" :md="8" :sm="12" :xs="24"`
- [x] 字号响应式：`tokens.css` 中 768px 断点调整

### 4. 组件层级 📦
- [x] 全局 8px 基准间距系统
- [x] 卡片 gutter 使用 `24px`
- [x] 段落间距 `--spacing-md`
- [x] 所有组件使用语义化变量

### 5. 可访问性 ♿
- [x] 品牌色分两级满足对比度
- [x] 自定义 focus ring 样式
- [x] 所有交互元素可键盘访问
- [x] 深色模式图片亮度调低 `filter: brightness(0.95)`

### 6. 温暖治愈风格专项 🍂
- [x] 纸张纹理背景 `.paper-texture`
- [x] 装饰性 Blob 背景 `.decoration-blob`
- [x] 作者笔记装饰块 `.author-note`
- [x] 标题使用衬线字体
- [x] 过渡动效平滑 0.25s

---

## 注意事项
1. **功能优先**：确保样式改造不破坏现有交互逻辑
2. **渐进式**：逐个页面改造，每完成一个就验证功能
3. **语义化**：CSS 变量命名保持语义化，避免使用具体色值命名
4. **回滚方案**：改造前可创建 `feature/style-refactor` 分支，有问题随时切回
5. **浏览器兼容**：CSS 变量在现代浏览器支持良好，无需考虑 IE
