<template>
  <div class="post-detail">
    <el-button class="back-btn" @click="$router.back()">
      <el-icon><ArrowLeft /></el-icon>
      返回列表
    </el-button>
    <el-card class="detail-card">
      <div class="detail-header">
        <h1 class="post-title">{{ post.title }}</h1>
        <el-button
          v-if="userStore.isLoggedIn && post.authorId === userStore.user?.id"
          class="delete-btn"
          size="small"
          @click="deletePost"
        >删除文章</el-button>
      </div>
      <div class="post-meta">
        <el-tag size="small">{{ post.categoryName }}</el-tag>
        <span class="author" @click="goToProfile(post.authorId)">{{ post.authorName || '匿名' }}</span>
        <span class="date">{{ formatDate(post.createdAt) }}</span>
        <span class="comment-count">
          <el-icon><ChatDotRound /></el-icon> {{ comments.length }} 条评论
        </span>
      </div>

      <div v-if="post.images?.length" class="post-images">
        <el-image
          v-for="(img, index) in post.images"
          :key="index"
          :src="getImageUrl(img.filePath)"
          fit="cover"
          :preview-src-list="post.images.map(i => getImageUrl(i.filePath))"
          :initial-index="index"
          class="post-image"
        />
      </div>

      <div class="post-content markdown-body" v-html="renderedContent"></div>
    </el-card>

    <el-card class="comments-card">
      <h3 class="comments-title">
        <el-icon><ChatDotRound /></el-icon>
        评论 ({{ comments.length }})
      </h3>

      <div class="comment-form">
        <el-input
          v-model="newComment.authorName"
          placeholder="您的昵称"
          class="comment-input"
          :disabled="userStore.isLoggedIn"
        />
        <el-input
          v-model="newComment.email"
          placeholder="邮箱（可选）"
          class="comment-input"
          :disabled="userStore.isLoggedIn"
        />
        <el-input
          v-model="newComment.content"
          type="textarea"
          :rows="3"
          placeholder="写下您的评论..."
          class="comment-textarea"
        />
        <el-button type="primary" @click="submitComment" :loading="submitting">
          发表评论
        </el-button>
      </div>

      <div class="comments-list">
        <div v-if="!comments.length" class="no-comments">
          <el-empty description="暂无评论，快来抢沙发吧~" />
        </div>
        <div v-for="comment in comments" :key="comment.id" class="comment-item">
          <div class="comment-avatar">
            <el-icon :size="32"><User /></el-icon>
          </div>
          <div class="comment-body">
            <div class="comment-header">
              <span class="comment-author">{{ comment.authorName }}</span>
              <span class="comment-date">{{ formatDate(comment.createdAt) }}</span>
            </div>
            <div class="comment-content">{{ comment.content }}</div>
            <div v-if="comment.reply" class="comment-reply">
              <div class="reply-label">作者回复：</div>
              <div class="reply-content">{{ comment.reply }}</div>
            </div>
            <div v-if="userStore.isLoggedIn && (post.authorId === userStore.user?.id || userStore.user?.isAdmin) && !comment.reply" class="comment-actions">
              <el-button size="small" link @click="showReplyForm(comment)">回复</el-button>
              <el-button size="small" link type="danger" @click="deleteComment(comment.id)">删除</el-button>
            </div>
            <div v-if="replyingTo === comment.id" class="reply-form">
              <el-input
                v-model="replyContent"
                type="textarea"
                :rows="2"
                placeholder="回复这条评论..."
              />
              <div class="reply-actions">
                <el-button size="small" @click="replyingTo = null">取消</el-button>
                <el-button size="small" type="primary" @click="submitReply(comment.id)" :loading="submittingReply">发送</el-button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowLeft, ChatDotRound, User } from '@element-plus/icons-vue'
import { postsApi } from '../api'
import { marked } from 'marked'
import { useUserStore } from '../stores/user'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const post = ref({ title: '', content: '', images: [] })
const comments = ref([])
const submitting = ref(false)
const submittingReply = ref(false)
const replyingTo = ref(null)
const replyContent = ref('')
const newComment = ref({
  authorName: '',
  email: '',
  content: ''
})

const renderedContent = computed(() => {
  return marked(post.value.content || '')
})

const getImageUrl = (path) => {
  if (!path) return ''
  return path.startsWith('http') ? path : 'http://localhost:5000' + path
}

const fetchPost = async () => {
  const res = await postsApi.getDetail(route.params.id)
  post.value = res
  await nextTick()
  if (window.MathJax && window.MathJax.typesetPromise) {
    window.MathJax.typesetPromise()
  }
}

const fetchComments = async () => {
  try {
    comments.value = await postsApi.getComments(route.params.id)
  } catch (e) {
    console.error('加载评论失败', e)
  }
}

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('zh-CN')
}

const goToProfile = (userId) => {
  if (userId) {
    router.push(`/profile/${userId}`)
  }
}

const deletePost = async () => {
  try {
    await ElMessageBox.confirm(`确定删除文章「${post.value.title}」吗？`, '提示', { type: 'warning' })
    const res = await postsApi.delete(route.params.id)
    if (res.success) {
      ElMessage.success('删除成功')
      router.push('/posts')
    }
  } catch {
    // 用户取消
  }
}

const submitComment = async () => {
  if (!newComment.value.content.trim()) {
    ElMessage.warning('请输入评论内容')
    return
  }

  if (!userStore.isLoggedIn && !newComment.value.authorName.trim()) {
    ElMessage.warning('请输入昵称')
    return
  }

  submitting.value = true
  try {
    const data = {
      postId: parseInt(route.params.id),
      guestName: userStore.isLoggedIn ? undefined : newComment.value.authorName,
      guestEmail: newComment.value.email || undefined,
      content: newComment.value.content
    }

    const res = await postsApi.createComment(data)
    if (res.success) {
      ElMessage.success('评论成功')
      newComment.value = { authorName: '', email: '', content: '' }
      fetchComments()
    }
  } catch (e) {
    ElMessage.error('评论失败')
  } finally {
    submitting.value = false
  }
}

const showReplyForm = (comment) => {
  replyingTo.value = comment.id
  replyContent.value = ''
}

const submitReply = async (commentId) => {
  if (!replyContent.value.trim()) {
    ElMessage.warning('请输入回复内容')
    return
  }

  submittingReply.value = true
  try {
    const res = await postsApi.replyComment(commentId, { reply: replyContent.value })
    if (res.success) {
      ElMessage.success('回复成功')
      replyingTo.value = null
      replyContent.value = ''
      fetchComments()
    }
  } catch (e) {
    ElMessage.error('回复失败')
  } finally {
    submittingReply.value = false
  }
}

const deleteComment = async (commentId) => {
  try {
    await ElMessageBox.confirm('确定删除这条评论吗？', '提示', { type: 'warning' })
    const res = await postsApi.deleteComment(commentId)
    if (res.success) {
      ElMessage.success('删除成功')
      fetchComments()
    }
  } catch {
    // 用户取消
  }
}

onMounted(() => {
  fetchPost()
  fetchComments()
})
</script>

<style scoped>
.post-detail {
  position: relative;
  z-index: 1;
}

.back-btn {
  margin-bottom: var(--spacing-lg);
  color: var(--text-secondary);
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.back-btn:hover {
  color: var(--brand-primary);
  background-color: var(--bg-secondary);
}

.detail-card {
  margin-top: var(--spacing-lg);
}

.delete-btn {
  color: var(--text-muted);
  border-color: var(--border-light);
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.delete-btn:hover {
  color: #D45D5D;
  border-color: #D45D5D;
  background-color: rgba(212, 93, 93, 0.1);
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.post-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-2xl);
  margin: 0 0 var(--spacing-lg);
  text-align: center;
  color: var(--text-primary);
}

.post-meta {
  text-align: center;
  color: var(--text-muted);
  margin-bottom: var(--spacing-xl);
  padding-bottom: var(--spacing-lg);
  border-bottom: 1px solid var(--border-light);
  display: flex;
  justify-content: center;
  align-items: center;
  gap: var(--spacing-md);
  flex-wrap: wrap;
}

.post-meta .author {
  color: var(--brand-primary);
  cursor: pointer;
  font-size: var(--font-size-sm);
  transition: color var(--transition-fast);
}

.post-meta .author:hover {
  color: var(--brand-primary-deep);
}

.post-meta .date {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

.comment-count {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  display: flex;
  align-items: center;
  gap: 4px;
}

.post-images {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-xl);
}

.post-image {
  width: 100%;
  height: 200px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: transform var(--transition-fast);
}

.post-image:hover {
  transform: scale(1.02);
}

.post-content {
  line-height: var(--line-height-reading);
  color: var(--text-primary);
  font-size: var(--font-size-base);
}

.comments-card {
  margin-top: var(--spacing-xl);
}

.comments-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-xl);
  margin: 0 0 var(--spacing-lg);
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-primary);
}

.comment-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding-bottom: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
  border-bottom: 1px solid var(--border-light);
}

.comment-input {
  max-width: 300px;
}

.comment-textarea {
  width: 100%;
}

.comments-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.no-comments {
  padding: var(--spacing-xl) 0;
}

.comment-item {
  display: flex;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--bg-secondary);
  border-radius: var(--radius-md);
}

.comment-avatar {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--bg-card);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
}

.comment-body {
  flex: 1;
  min-width: 0;
}

.comment-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-sm);
}

.comment-author {
  font-weight: 500;
  color: var(--text-primary);
}

.comment-date {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.comment-content {
  color: var(--text-secondary);
  line-height: 1.6;
  margin-bottom: var(--spacing-sm);
}

.comment-reply {
  background: var(--bg-card);
  border-left: 3px solid var(--brand-primary);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: 0 var(--radius-sm) var(--radius-sm) 0;
  margin: var(--spacing-sm) 0;
}

.reply-label {
  font-size: var(--font-size-xs);
  color: var(--brand-primary);
  font-weight: 500;
  margin-bottom: 4px;
}

.reply-content {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.6;
}

.comment-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.reply-form {
  margin-top: var(--spacing-sm);
}

.reply-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
  margin-top: var(--spacing-sm);
}

.markdown-body h1, .markdown-body h2, .markdown-body h3,
.markdown-body h4, .markdown-body h5, .markdown-body h6 {
  margin-top: 1.5em;
  margin-bottom: 0.5em;
  font-family: var(--font-family-serif);
}

.markdown-body p {
  margin-bottom: 1em;
}

.markdown-body ul, .markdown-body ol {
  padding-left: 2em;
  margin-bottom: 1em;
}

.markdown-body code {
  background: var(--bg-secondary);
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.9em;
}

.markdown-body pre {
  background: var(--bg-secondary);
  padding: 1em;
  border-radius: 8px;
  overflow-x: auto;
  margin-bottom: 1em;
}

.markdown-body pre code {
  background: none;
  padding: 0;
}

.markdown-body blockquote {
  border-left: 4px solid var(--brand-primary);
  padding-left: 1em;
  margin: 1em 0;
  color: var(--text-secondary);
}

.markdown-body img {
  max-width: 100%;
  border-radius: 8px;
}

.markdown-body a {
  color: var(--brand-primary);
}

.markdown-body table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 1em;
}

.markdown-body th, .markdown-body td {
  border: 1px solid var(--border-light);
  padding: 8px 12px;
  text-align: left;
}

.markdown-body mjx-container {
  overflow-x: auto;
  max-width: 100%;
  display: inline-block !important;
}

.markdown-body mjx-container[display="true"] {
  display: block !important;
  margin: 1em 0;
  text-align: center;
}

@media (max-width: 768px) {
  .post-images {
    grid-template-columns: repeat(2, 1fr);
  }

  .comment-input {
    max-width: 100%;
  }
}
</style>
