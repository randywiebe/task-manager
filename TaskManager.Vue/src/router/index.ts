import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import Tasks from '../components/Tasks.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/lists/:id',
      name: 'tasks',
      component: Tasks,
      props: true,
    },
  ],
})

export default router
