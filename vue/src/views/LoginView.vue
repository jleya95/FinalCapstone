<template>
  <h2 class="sign-in">&nbsp;&nbsp;Please Sign In</h2>
  <div class="divClass" id="login">
    <form class="log-in" v-on:submit.prevent="login">
      <div role="alert" v-if="invalidCredentials">
        <strong>Invalid username and password!</strong>
      </div>
      <div role="alert" v-if="this.$route.query.registration">
        Thank you for registering, please sign in.
      </div>
      <div class="form-input-group">
        <label for="username">Username </label>
        <input class="input-box" type="text" id="username" v-model="user.username" required autofocus />
      </div>
      <div class="form-input-group">
        <label for="password">Password &nbsp;</label>
        <input class="input-box" type="password" id="password" v-model="user.password" required  />
      </div>
      <button class="button" id="premiumButton" @click="LibraryPagePush()">Sign In</button>
      <!-- <p>
        <router-link v-bind:to="{ name: 'register' }">Need an account? Sign up.</router-link>
      </p> -->
    </form>
  </div>
  
</template>

<script>
import authService from "../services/AuthService";


export default {
  data() {
    return {
      user: {
        username: "",
        password: ""
      },
      invalidCredentials: false
    };
  },
  methods: {
    login() {
      authService
        .login(this.user)
        .then(response => {
          if (response.status == 200) {
            this.$store.commit("SET_AUTH_TOKEN", response.data.token);
            this.$store.commit("SET_USER", response.data.user);
            this.$router.push("/");
          }
        })
        .catch(error => {
          const response = error.response;

          if (response.status === 401) {
            this.invalidCredentials = true;
          }
        });
    },
  }
};
</script>

<style scoped>
.sign-in {
  color: white;
  text-align: center;
}
.form-input-group {
  margin-bottom: 1rem;
  color: white;
  padding-left: 0px;
}

label {
  margin-right: 0.5rem;
  margin-left: 10px;
}
.input-box {
  background-color: #000000;
  color: white;
  
}

#login {
  display: flex;
  position: absolute;
  padding-top: 50px;
  left: 820px;
}

.img {
  width: 550px;
  height: 500px;
  margin-left: 0%;
  max-height: none;
}

.log-in {
  margin-left: 0%;
  color: white;
}
.divClass{
  display: flex;
  width: 280px;
  height: 180px;
  color: white;
  position: relative;
}
h2{
  padding-top: 30px;
  color: black;
}

.button {
 --color: white;
 font-family: inherit;
 display: inline-block;
 width: 15.3em;
 height: 2.6em;
 line-height: 1.5em;
 margin: 10px;
 position: relative;
 overflow: hidden;
 border: 2px solid var(--color);
 transition: color .5s;
 z-index: 1;
 font-size: 17px;
 border-radius: 8px;
 font-weight: 500;
 color: var(--color);
 background-color: black;
}

.button:before {
 content: "";
 position: absolute;
 z-index: -1;
 background: var(--color);
 height: 115px;
 width: 500px;
 border-radius: 50%;
}

.button:hover {
 color: black;
}

.button:before {
 top: 100%;
 left: -100%;
 transition: all .7s;
}

.button:hover:before {
 top: -30px;
 left: -30px;
 background-color: #EEE810;
}

.button:active:before {
 background: #B856AB;
 transition: background 0s;
}


</style>