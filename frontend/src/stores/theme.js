import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', {
  state: () => ({
    currentTheme: 'light' // 'light' | 'dark' | 'system'
  }),

  actions: {
    /**
     * 初始化主题 - 应用启动时调用
     * 优先级：localStorage 用户偏好 > 系统主题
     */
    initTheme() {
      const savedTheme = localStorage.getItem('user-theme')

      if (savedTheme) {
        this.currentTheme = savedTheme
      } else {
        // 检测系统主题偏好
        this.currentTheme = window.matchMedia('(prefers-color-scheme: dark)').matches
          ? 'dark'
          : 'light'
      }

      this.applyTheme()
      this.watchSystemThemeChange()
    },

    /**
     * 切换主题
     */
    setTheme(theme) {
      this.currentTheme = theme
      localStorage.setItem('user-theme', theme)
      this.applyTheme()
    },

    /**
     * 应用主题到 DOM
     */
    applyTheme() {
      let effectiveTheme = this.currentTheme

      // 如果是跟随系统，计算实际主题
      if (this.currentTheme === 'system') {
        effectiveTheme = window.matchMedia('(prefers-color-scheme: dark)').matches
          ? 'dark'
          : 'light'
      }

      if (effectiveTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark')
      } else {
        document.documentElement.removeAttribute('data-theme')
      }
    },

    /**
     * 监听系统主题变化
     */
    watchSystemThemeChange() {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')

      mediaQuery.addEventListener('change', () => {
        // 只有在跟随系统模式下才自动切换
        if (this.currentTheme === 'system') {
          this.applyTheme()
        }
      })
    },

    /**
     * 快捷切换：浅色 <-> 深色
     */
    toggleTheme() {
      const nextTheme = this.currentTheme === 'light' ? 'dark' : 'light'
      this.setTheme(nextTheme)
    }
  }
})
