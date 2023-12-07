import { createRouter as createRouter, createWebHistory } from 'vue-router'
import { useStore } from 'vuex'

// Import components
import HomeView from '../views/HomeView.vue';
import LoginView from '../views/LoginView.vue';
import LogoutView from '../views/LogoutView.vue';
import RegisterView from '../views/RegisterView.vue';
import SearchBox from '../components/SearchBox.vue';
import Library from'../views/Library.vue'
import Collections from '@/views/Collections.vue'
import Data from '@/views/Data.vue'
import SearchResult from'@/views/SearchResult.vue'
import Caorusel from '@/views/CaorusalView.vue'
import ProfilePageView from '@/views/ProfilePageView.vue'
/**
 * The Vue Router is used to "direct" the browser to render a specific view component
 * inside of App.vue depending on the URL.
 *
 * It also is used to detect whether or not a route requires the user to have first authenticated.
 * If the user has not yet authenticated (and needs to) they are redirected to /login
 * If they have (or don't need to) they're allowed to go about their way.
 */
const routes = [
  {
    path: '/home',
    name: 'home',
    component: HomeView,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/login",
    name: "login",
    component: LoginView,
    meta: {
      requiresAuth: false
    }
  },
  {
    path: "/logout",
    name: "logout",
    component: LogoutView,
    meta: {
      requiresAuth: false
    }
  },
  {
    path: "/register",
    name: "register",
    component: RegisterView,
    meta: {
      requiresAuth: false
    }
  },
  {
    path: "/search",
    name: "search",
    component: SearchBox,
    meta: {
      requiresAuth: false
    }
  },
  {
    path:"/Library",
    name: "Library",
    component: Library,
    meta: {
      requiresAuth: false
    }
  },
  {
    path:"/collections",
    name: "Collections",
    component: Collections,
    meta: {
      requiresAuth: false,
    }
  },
  {
    path: "/data",
    name: "data",
    component: Data,
    meta: {
      requiresAuth: false,
    }
  },
  {
    path: "/SearchResult",
    name: "SearchResult",
    component: SearchResult,
    meta: {
      requiresAuth: false,
    } 
    
  },
  {
    path: "/Profile",
    name: "Profile",
    component: ProfilePageView,
    meta: {
      requiresAuth: false,
    } 
  },
  {
    path: "/Caorusel",
    name: "Caorusel",
    component: Caorusel,
    meta: {
      requiresAuth: false,
    } 
  }
];

// Create the router
const router = createRouter({
  history: createWebHistory(),
  routes: routes
});

router.beforeEach((to) => {

  // Get the Vuex store
  const store = useStore();

  // Determine if the route requires Authentication
  const requiresAuth = to.matched.some(x => x.meta.requiresAuth);

  // If it does and they are not logged in, send the user to "/login"
  if (requiresAuth && store.state.token === '') {
    return {name: "login"};
  }
  // Otherwise, do nothing and they'll go to their next destination
});

export default router;
