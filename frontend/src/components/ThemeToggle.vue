<template>
  <el-tooltip :content="themeStore.currentTheme === 'light' ? '切换深色模式' : '切换浅色模式'" placement="bottom">
    <button
      class="theme-toggle"
      @click="themeStore.toggleTheme()"
      :aria-label="themeStore.currentTheme === 'light' ? '切换到深色模式' : '切换到浅色模式'"
    >
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
  flex-shrink: 0;
}

.theme-toggle:hover {
  background-color: var(--bg-secondary);
  color: var(--brand-primary);
  transform: scale(1.05);
}

.theme-toggle:active {
  transform: scale(0.95);
}

.theme-toggle:focus-visible {
  outline: 2px solid var(--brand-primary);
  outline-offset: 2px;
}
</style>
