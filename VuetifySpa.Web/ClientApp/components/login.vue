﻿<template>
    <v-container fluid fill-height>
        <v-layout align-center justify-center>
            <v-flex xs12 sm7 md4 lg3>

                <v-card v-if="loginPage" class="elevation-12">
                    <form id="login" @submit.prevent="onSubmit">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Форма входа</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>

                            <v-text-field prepend-icon="email"
                                          v-validate="'required|email'"
                                          :error-messages="errors.collect('login_email')"
                                          data-vv-name="email"
                                          required
                                          v-model="login.email" name="login_email" label="Email" type="text"></v-text-field>
                            <v-text-field prepend-icon="lock"
                                          v-validate="'required|min:4|max:20'"
                                          :error-messages="errors.collect('password')"
                                          data-vv-name="password"
                                          data-vv-as="пароль"
                                          required
                                          v-model="login.password" name="password" label="Пароль" type="password"></v-text-field>

                        </v-card-text>

                        <v-card-actions>

                            <v-layout align-center justify-center wrap>
                                <v-btn style="margin:15px;" type="submin" color="primary" :loading="loading" :disable="loading">войти</v-btn>


                                <v-btn tag="a" v-on:click="$router.push({ name: 'register' })" color="yellow"> регистрация</v-btn>
                                </v-layout>
</v-card-actions>
                    </form>
                </v-card>
                <v-card v-if="!loginPage" class="elevation-12">
                    <form id="reg" @submit.prevent="onSubmit">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Форма регистрации</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>

                            <v-text-field v-model="reg.email"
                                          v-validate="'required|regex_email|unique_email|max:120'"
                                          :error-messages="errors.collect('reg_email')"
                                          autocomplete="off"
                                          data-vv-as="email"
                                          required prepend-icon="email" name="reg_email" id="reg_email" label="Email"></v-text-field>
                            <v-text-field v-model="reg.firstName"
                                          v-validate="'required|min:3|max:60'"
                                          :error-messages="errors.collect('reg_firstName')"
                                          autocomplete="off"
                                          data-vv-as="имя"
                                          required prepend-icon="email" name="reg_firstName" id="reg_firstName" label="Имя"></v-text-field>
                            <v-text-field v-model="reg.lastName"
                                          v-validate="'required|min:3|max:60'"
                                          :error-messages="errors.collect('reg_lastName')"
                                          autocomplete="off"
                                          data-vv-as="фамилия"
                                          required prepend-icon="email" name="reg_lastName" id="reg_lastName" label="Фамилия"></v-text-field>
                            <v-text-field v-model="reg.password"
                                          v-validate="'required|min:4|max:20'"
                                          :error-messages="errors.collect('password')"
                                          data-vv-name="password"
                                          data-vv-as="пароль"
                                          autocomplete="off"
                                          required
                                          prepend-icon="lock" label="Пароль" name="reg_password" type="password"></v-text-field>

                            <v-text-field v-model="reg.repassword"
                                          v-validate="'required|confirmed:reg_password'"
                                          :error-messages="errors.collect('reg_repassword')"
                                          data-vv-name="repassword"
                                          autocomplete="off"
                                          data-vv-as="повторить пароль"
                                          required prepend-icon="lock" name="reg_repassword" label="Повторите пароль" type="password"></v-text-field>

                        </v-card-text>

                        <v-card-actions>
                            <v-layout align-center justify-center wrap>
                           
                                <v-btn style="margin:15px" tag="a" v-on:click="$router.push({ name: 'login' })" color="yellow">войти c email и паролем</v-btn> 
                                <v-btn type="submit" color="primary" :loading="loading" :disable="loading">зарегистрироваться</v-btn>
                                </v-layout>
                        </v-card-actions>
                    </form>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script>
    import Vue from 'vue'

    import { fail } from 'assert';
    import { HasEmptyJson } from "../app.js"
    import { isNullOrEmpty } from "../app.js"

    import { test } from '../store/error_store.js';




    export default {
        $_veeValidate: {
            validator: 'new'
        },
        props: {
            GetAuthFunc: Function,
            loginPage: {
                type: Boolean,
                default: false,
            }
        },  
        data() {
            return {
           
                login: {
                    email: 'admin@test.ru',
                    password: 'LikeMe123!',
                },
                reg: {
                    email: '',
                    firstName: '',
                    lastName: '',
                    password: '',
                    repassword: '',
                },
                returnUrl: '',
            }
        },
        computed: {
            loading: function () {
                cache: false;
                return this.$store.getters.Getloading;
            },
            getUser() {
                cache: false;
                return this.$store.getters.GetAuthUser;
            },
        },
        mounted() {

            //this.$validator.localize('ru', VeeValidateRu)   



            if (this.$route.query.access != undefined) {
                var message = "";
                this.returnUrl = this.$route.query.returnUrl
                if (this.$route.query.access == 'admin') {
                    message = "пожалуйста, вы должны авторизоваться под ролью админитратора";
                } else
                    if (this.$route.query.access == 'user') {
                        message = "пожалуйста, вы должны авторизоваться";
                    }
                this.$store.dispatch('SET_ERROR', message);

            }
        },
        methods: {
            onSubmit() {
                this.$validator.validateAll().then((result) => {

                    if (result) {
                        var formName = this.loginPage ? 'loginAuth' : 'regAuth';
                        var formData = this.loginPage ? this.login : this.reg;

                        //var action = this.loginPage ? { name: 'loginAuth', data: this.login }
                        //    : { name: 'regAuth', data: this.reg }

                        var gg = this.$store.dispatch(formName, { data: formData });
                        gg.then(() => {
                            if (this.$store.state.authUser) {
                                var routerPath = "/"
                                if (!HasEmptyJson(this.$route.query.returnUrl)) {
                                    routerPath = this.$route.query.returnUrl;
                                }
                                this.$router.push(routerPath);
                            }
                        });
                    }
                });
            }
        },
        created: function () {
           
        }
    }
</script>

<style>
</style>
