<template>
    <div id="register" class="text-center">
        <form class="registrationForm" v-on:submit.prevent="register">
            <h1>Create Account</h1>
            <div role="alert" v-if="registrationErrors">
                {{ registrationErrorMsg }}
            </div>
            <div id="buy-grid">
                <button class="box-premium" >
                    <box class="box-pre">
                        <div class="media-content" @click="premiumRegistration">
                            <div class="content">
                                <div class="myClass animate__animated animate__hoveranimate__bounce"></div>
                                <h2><strong> <i class="fa-solid fa-peace" style="color: #363636"></i> CLICK FOR PREMIUM</strong>
                                </h2>
                                <h4>
                                    <p style="color: #363636"><i class="fa-solid fa-floppy-disk" style="color: #363636"></i>&nbsp;&nbsp;Save unlimited records to your
                                        library
                                    </p>
                                    <p style="color: #363636"><i class="fa-solid fa-infinity" style="color: #363636"></i>&nbsp;&nbsp;Create unlimited collections</p>
                                </h4>
                            </div>
                        </div>
                    </box>
                </button>
                <div class="boxes" style="grid-area: inputBoxes">
                    <p style="grid-area: firstNameLabel">First Name: </p>
                    <div class="form-input-group">
                        <input type="text" id="firstName" v-model="user.first_Name" required autofocus />
                    </div>
                    <p style="grid-area: lastNameLabel">Last Name: </p>
                    <div class="form-input-group">
                        <input type="text" id="lastName" v-model="user.last_Name" required autofocus />
                    </div>
                    <p style="grid-area: emailLabel">Email: </p>
                    <div class="form-input-group">
                        <input type="text" id="email" v-model="user.email_Address" required />
                    </div>
                    <p style="grid-area:usernameLabel">Username: </p>
                    <div class="form-input-group">
                        <input type="text" id="username" v-model="user.username" required />
                    </div>
                    <p style="grid-area: passwordLabel">Password: </p>
                    <div class="form-input-group">
                        <input type="password" id="password" v-model="user.password" required />
                    </div>
                    <p style="grid-area: confirmPasswordLabel">Confirm Password: </p>
                    <div class="form-input-group">
                        <input type="password" id="confirmPassword" v-model="user.confirmPassword" required />
                    </div>
                </div>
                <button class="box-free">
                    <box class="box-pre">
                        <div class="media-content" @click="freeRegistration">
                            <div class="content">
                                <div class="myClass animate__animated animate__hoveranimate__bounce"></div>
                                <h2><strong> <i class="fa-solid fa-crown" style="color: #363636"></i> CLICK FOR BASIC</strong>
                                </h2>
                                <h4>
                                    <p style="color: #363636"><i class="fa-solid fa-floppy-disk" style="color: #363636"></i>&nbsp;&nbsp;Save records to your library</p>
                                    <p style="color: #363636"><i class="fa-solid fa-box" style="color: #363636"></i>&nbsp;&nbsp;Create up to 25 collections</p>
                                    <p style="color: #363636"><i class="fa-solid fa-record-vinyl" style="color: #363636"></i>&nbsp;&nbsp;Browse various records</p>
                                    <p></p>
                                </h4>
                            </div>
                        </div>
                    </box>
                </button>
            </div>
        </form>

    </div>
</template>

<script>
import authService from '@/services/AuthService';
import UserFunctionsService from '@/services/UserFunctionsService.js'

export default {
    data() {
        return {
            user: {
                first_Name: '',
                last_Name: '',
                email_Address: '',
                username: '',
                password: '',
                confirmPassword: '',
                role: '',
            },
            registrationErrors: false,
            registrationErrorMsg: 'There were problems registering this user.',
        };
    },
    methods: {
        register() {
            if (this.user.password != this.user.confirmPassword) {
                this.registrationErrors = true;
                this.registrationErrorMsg = 'Password & Confirm Password do not match.';
            } else {
                authService
                    .register(this.user)
                    .then((response) => {
                        if (response.status == 201) {
                            this.$router.push({
                                path: '/home',
                                query: { registration: 'success' },
                            });
                        }
                    })
                    .catch((error) => {
                        const response = error.response;
                        this.registrationErrors = true;
                        if (response.status === 400) {
                            this.registrationErrorMsg = 'Bad Request: Validation Errors';
                        }
                    });
            }
        },
        clearErrors() {
            this.registrationErrors = false;
            this.registrationErrorMsg = 'There were problems registering this user.';
        },
        LibraryPagePush() {
            this.$router.push({ name: "library" });
        },
        freeRegistration() {
            this.user.role = 'free';
            this.register();
        },
        premiumRegistration() {
            this.user.role = 'premium';
            this.register();
        }
    },
};

</script>

<style scoped>
.form-input-group {
    color: white;
    padding: 10px;
}


.form-input-group2 {
    color: white;

}

.boxes {
    display: grid;
    align-items: center;
    grid-template-columns: 1fr 1fr;
    grid-template-areas:
        "firstNameLabel firstName"
        "lastNameLabel lastName"
        "emailLabel email"
        "usernameLabel username"
        "passwordLabel password"
        "confirmPasswordLabel confirmPassword";
    justify-content: center;
    position: relative;
    flex-direction: column;
    text-align: left;
    width: 59%;
}

.registrationForm {
    display: flex;
    flex-direction: column;
    align-items: center;
}

#email {
    background-color: black;
    color: white;
    grid-area: email;


}

#firstName {
    background-color: black;
    color: white;
    grid-area: firstName;


}

#lastName {
    background-color: black;
    color: white;
    grid-area: lastName;

}


#username {
    background-color: black;
    color: white;
    grid-area: username;


}

#password {
    background-color: black;
    color: white;
    grid-area: password;
}

#confirmPassword {
    background-color: black;
    color: white;
    grid-area: confirmPassword;

}

label {
    margin-right: 0.5rem;
}

.box-pre {
    padding: 10px;
}

#buy-grid {
    display: grid;
    display: flex;
    grid-template-columns: 1fr 1fr 1fr;
    grid-template-areas:
        "box-premium inputBoxes box-free";
    row-gap: 20px;
    column-gap: 22px;
    align-items: center;
    padding-top: 20px;
    padding-bottom: 25px;
    max-width: 1000px;
}

.box-premium {
    display: grid;
    grid-area: box-premium;
    display: flex;
    flex-direction: row;
    width: 500px;
    height: 255px;
    border: 5px solid rgb(255, 255, 255);
    background-image: linear-gradient(#EEE810, #f5f6c9);
    border-radius: 10px;
    justify-content: left;
    transition: transform 0.3s ease;
    text-align: left;

}

.box-premium:hover {
    transition: transform 0.3s ease;
    transform: translateY(-10px);
}

.box-free {
    display: grid;
    grid-area: box-free;
    display: flex;
    flex-direction: row;
    width: 500px;
    border: 5px solid rgb(255, 255, 255);
    background-image: linear-gradient(#08A2D9, rgb(176, 221, 237));
    border-radius: 10px;
    text-align: left;
    transition: transform 0.3s ease;
}

.box-free:hover {
    transition: transform 0.3s ease;
    transform: translateY(-10px);
}


h4 {
    color: white;
}

.fa-solid {
    color: white;
    padding-top: 15px;
}

.button2 {
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

.button2:before {
    content: "";
    position: absolute;
    z-index: -1;
    background: var(--color);
    height: 115px;
    width: 500px;
    border-radius: 50%;
}

.button2:hover {
    color: black;
}

.button2:before {
    top: 100%;
    left: 100%;
    transition: all .7s;
}

.button2:hover:before {
    top: -30px;
    left: -30px;
    background-color: #05B5A1;
}

.button2:active:before {
    background: #B856AB;
    transition: background 0s;
}
</style>
