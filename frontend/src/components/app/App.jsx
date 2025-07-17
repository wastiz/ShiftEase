import {BrowserRouter, Routes, Route} from 'react-router-dom';
import Landing from '../landing/Landing.jsx';
import AuthRoot from "../auth/AuthRoot.jsx";
import Base from "../employerView/base/Base.jsx"
import OrganizationAddForm from "../employerView/organizations/OrganizationAddForm.jsx";
import Organizations from "../employerView/organizations/Organizations.jsx";
import './App.css';
import ErrorBoundary from "../assets/ErrorBoundary.jsx";
import EmployeeScheduleView from "../employeeView/scheduleView/EmployeeScheduleView.jsx"
import EmployeePersonalPage from "../employeeView/EmployeePersonalPage.jsx";
import ProtectedRoute from '../assets/ProtectedRoute.jsx';

function App() {
    
    return (
        <BrowserRouter>
            <Routes>
                {/* Public Routes */}
                <Route exact path='/' element={<Landing/>}></Route>
                <Route exact path='/login' element={<AuthRoot/>}></Route>

                {/* Protected Routes */}
                <Route element={<ProtectedRoute />}>
                    <Route path="/organizations" element={<Organizations />} />
                    <Route path="/add-organization" element={<OrganizationAddForm />} />
                    <Route path="/edit-organization/:orgId" element={<OrganizationAddForm />} />
                    <Route path="/organization/:orgId/*" element={<Base />} />
                </Route>

                <Route exact path='/employee-personal-page' element={<EmployeePersonalPage/>}></Route>
            </Routes>
        </BrowserRouter>
                
    );
}

export default App;
