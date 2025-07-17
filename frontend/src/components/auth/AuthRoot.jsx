import { useNavigate } from 'react-router-dom';
import './AuthRoot.css';
import { useState } from 'react';
import { motion, AnimatePresence } from "framer-motion";
import { useForm } from "react-hook-form";
import Button from "react-bootstrap/Button";
import Form from 'react-bootstrap/Form';
import LoadingSpinner from '../assets/LoadingSpinner.jsx';
import ToggleGroup from "../assets/ToggleGroup.jsx";
import TextInput from "../assets/Inputs/TextInput.jsx";
import publicApi from "../../publicApi.js";

function AuthRoot() {
    const navigate = useNavigate();
    const [isLogin, setIsLogin] = useState(true);
    const [isEmployer, setIsEmployer] = useState(true);
    const { register, handleSubmit, formState: { errors }, setError, clearErrors, unregister } = useForm();
    const [isLoading, setLoading] = useState(false);

    const switchForm = () => {
        setIsLogin(!isLogin);
        unregister();
        clearErrors();
    }

    const switchRole = () => {
        setIsEmployer(!isEmployer);
        clearErrors();
    }

    //Login
    const handleLoginSubmit = async (data) => {
        clearErrors("common");
        setLoading(true);
        if (!isEmployer) {
            try {
                const response = await fetch(`/api/identity/login/employee`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: data.email,
                        password: data.password,
                    }),
                });

                const result = await response.json();
                setLoading(false);
                if (response.ok) {
                    localStorage.setItem("accessToken", result.accessToken);
                    localStorage.setItem("refreshToken", result.refreshToken);
                    localStorage.setItem("employeeId", result.employeeId);
                    localStorage.setItem("orgId", result.organizationId);
                    localStorage.setItem("groupId", result.groupId);
                    navigate("/employee-personal-page");
                } else {
                    setError("common", {type: "manual", message: result.message});
                }
            } catch (error) {
                setLoading(false);
                setError("common", {type: "manual", message: "An error occurred, please try again"});
            }
        } else {
            try {
                const response = await fetch(`/api/identity/login/employer`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: data.email.trim(),
                        password: data.password.trim(),
                    }),
                });

                const result = await response.json();
                setLoading(false);
                if (response.ok) {
                    localStorage.setItem("accessToken", result.accessToken);
                    localStorage.setItem("refreshToken", result.refreshToken);
                    navigate("/organizations")
                } else {
                    setError("common", {type: "manual", message: result.message});
                }
            } catch (error) {
                setLoading(false);
                setError("common", {type: "manual", message: "An error occurred, please try again"});
            }
        }
    };

    // Register
    const handleRegisterSubmit = async (data) => {
        clearErrors("common");
        setLoading(true);

        if (data.password !== data.repeatPassword) {
            setError("repeatPassword", { type: "manual", message: "Passwords do not match" });
            return setLoading(false);
        }

        const payload = {
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email.trim(),
            phone: data.phone.trim(),
            password: data.password.trim(),
        };

        try {
            const response = await publicApi.post("/identity/register/employer", payload);
            const result = response.data;

            localStorage.setItem("accessToken", result.accessToken);
            localStorage.setItem("refreshToken", result.refreshToken);

            navigate("/organizations");
        } catch (error) {
            const message =
                error.response?.data?.message || "An error occurred, please try again";
            setError("common", { type: "manual", message });
        } finally {
            setLoading(false);
        }
    };


    const handleChange = () => {
        clearErrors("common");
        clearErrors("repeatPassword");
    };

    return (
        <div className='auth-page'>
            <div className="auth-overlay">
                <header className='greeting-header'>ShiftEase</header>
                <motion.h2
                    key={isLogin ? "login" : "registration"}
                    className='greeting-text'
                    initial={{ opacity: 0, y: -10, filter: "blur(5px)" }}
                    animate={{ opacity: 1, y: 0, filter: "blur(0px)" }}
                    exit={{ opacity: 0, y: 10, filter: "blur(5px)" }}
                    transition={{ duration: 0.5 }}
                >
                    {isLogin ? "Welcome back!" : "Join us now!"}
                </motion.h2>
            </div>

            <AnimatePresence mode='wait'>
                <motion.div
                    key={isLogin ? 'login' : 'registration'}
                    className='auth-motion'
                    initial={{ opacity: 0, x: "-50vw" }}
                    animate={{ opacity: 1, x: "10vw" }}
                    exit={{ opacity: 0, x: "-50vw" }}
                    transition={{ duration: 0.5 }}
                >
                    <Form
                        className="form-primary"
                        onSubmit={isLogin ? handleSubmit(handleLoginSubmit) : handleSubmit(handleRegisterSubmit)}
                    >
                        <Form.Label column="lg" className="mb-3 text-center">{isLogin ? "Login" : "Registration"}</Form.Label>

                        {isLogin && (
                            <ToggleGroup
                                className="mb-3"
                                name="userType"
                                value={isEmployer ? 1 : 2}
                                onChange={(val) => setIsEmployer(val === 1)}
                                idPrefix="user"
                                options={[
                                    { label: "Employer", value: 1, onClick: switchRole },
                                    { label: "Employee", value: 2, onClick: switchRole },
                                ]}
                            />
                        )}

                        {!isLogin && (
                            <>
                                <TextInput
                                    className="mb-3"
                                    label="First Name"
                                    name="firstName"
                                    type="text"
                                    register={register}
                                    errors={errors}
                                    required="Enter First Name!"
                                    onChange={handleChange}
                                />
                                <TextInput
                                    className="mb-3"
                                    label="Last Name"
                                    name="lastName"
                                    type="text"
                                    register={register}
                                    errors={errors}
                                    required="Enter Last Name!"
                                    onChange={handleChange}
                                />
                            </>
                        )}

                        <TextInput
                            className="mb-3"
                            label="Email"
                            name="email"
                            type="email"
                            register={register}
                            errors={errors}
                            required="Enter email!"
                            onChange={handleChange}
                        />

                        {!isLogin && (
                            <TextInput
                                className="mb-3"
                                label="Phone"
                                name="phone"
                                type="phone"
                                register={register}
                                errors={errors}
                                onChange={handleChange}
                            />
                        )}

                        <TextInput
                            className="mb-3"
                            label="Password"
                            name="password"
                            type="password"
                            register={register}
                            errors={errors}
                            required="Enter password!"
                            onChange={handleChange}
                        />

                        {!isLogin && (
                            <TextInput
                                className="mb-3"
                                label="Repeat Password"
                                name="repeatPassword"
                                type="password"
                                register={register}
                                errors={errors}
                                required="Repeat your password!"
                                onChange={handleChange}
                            />
                        )}

                        <LoadingSpinner visible={isLoading} />

                        {errors.common && (
                            <p className="alert-danger mb-3">
                                {errors.common.message}
                            </p>
                        )}

                        <Button variant="primary" type="submit">
                            {isLogin ? "Log in" : "Sign up"}
                        </Button>

                        {isEmployer ? (
                            <p className="mt-3 text-center">
                                {isLogin ? "Don't have an account?" : "Already have an account?"}
                                <span onClick={switchForm} className="link-text ms-2">
                                {isLogin ? "Register" : "Login"}
                                </span>
                            </p>
                        ) : <p className="mt-3 text-center">Employee registration is restricted to employers</p>}

                    </Form>
                </motion.div>
            </AnimatePresence>
        </div>
    );
}

export default AuthRoot;
