import './Base.css';
import { useSelector } from 'react-redux';
import { Routes, Route } from 'react-router-dom';
import ViewSchedules from '../viewSchedules/viewSchedules.jsx';
import Schedules from '../manageSchedules/Schedules.jsx';
import ManageSchedule from '../manageSchedules/ManageSchedule.jsx';
import Employees from '../employees/Employees.jsx';
import Groups from '../groups/Groups.jsx';
import ShiftTypes from '../shiftTypes/ShiftTypes.jsx';
import Navbar from './navbar/Navbar.jsx';
import SupportForm from "../../assets/SupportForm/SupportForm.jsx";
import {LuLogOut} from "react-icons/lu";

function Base () {

    const { expandedNavbar } = useSelector((state) => state.common);
    return (
        <>
            <Navbar />
            <main style={{ width: expandedNavbar ? '90%' : '95%' }}>
                <Routes>
                    <Route path='/shift-table' element={<ViewSchedules/>}></Route>
                    <Route path='/manage-shifts/:groupId' element={<ManageSchedule/>}></Route>
                    <Route path='/schedules' element={<Schedules/>}></Route>
                    <Route path='/shifts' element={<ShiftTypes/>}></Route>
                    <Route path='/employees' element={<Employees/>}></Route>
                    <Route path='/groups' element={<Groups/>}></Route>
                    <Route path='/support' element={<SupportForm/>}></Route>
                </Routes>
            </main>
        </>
    )
}

export default Base;