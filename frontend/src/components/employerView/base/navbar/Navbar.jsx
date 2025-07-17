import './Navbar.css';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useLocation, useParams } from 'react-router-dom';
import { setExpandedNavbar } from '../../../../redux/slices/commonSlice.js';
import leftArrow from '../../../../img/icons/left-arrow.svg'
import menuIcon from '../../../../img/icons/menu.svg'
import { FaRegCalendarAlt } from "react-icons/fa";
import {LuLogOut, LuUsers} from "react-icons/lu";
import { TbCalendarUser } from "react-icons/tb";
import { FaExchangeAlt } from "react-icons/fa";
import { HiMiniUserGroup } from "react-icons/hi2";

const NavItem = ({ className, to, IconComponent, label }) => {
	const location = useLocation();
	const isActive = location.pathname === to;

	return (
		<Link to={to} className={`nav-item ${className} ${isActive ? 'active' : ''}`}>
			{IconComponent && <IconComponent className="nav-icon" />}
			{label}
		</Link>
	);
};

const Navbar = () => {
	const dispatch = useDispatch();
	const { expandedNavbar } = useSelector((state) => state.common);
	const { orgId } = useParams();

	return (
		<nav style={{ width: expandedNavbar ? '10%' : '5%' }}>
			{expandedNavbar ? (
				<div className='minimize-icon nav-item mb-2' onClick={() => dispatch(setExpandedNavbar())}>
					Minimize<img src={leftArrow} alt="" />
				</div>
			) : (
				<img className='nav-item menu-icon mb-2' src={menuIcon} alt='' onClick={() => dispatch(setExpandedNavbar())} />
			)}
			<div className='nav-item logo mb-2'>Logo</div>
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={`/organization/${orgId}/shift-table`} IconComponent={FaRegCalendarAlt} label={expandedNavbar ? "Shift Table" : ""} />
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={`/organization/${orgId}/schedules`} IconComponent={TbCalendarUser} label={expandedNavbar ? "Schedules" : ""} />
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={`/organization/${orgId}/shifts`} IconComponent={FaExchangeAlt} label={expandedNavbar ? "Shift Types" : ""} />
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={`/organization/${orgId}/employees`} IconComponent={LuUsers} label={expandedNavbar ? "Employees" : ""} />
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={`/organization/${orgId}/groups`} IconComponent={HiMiniUserGroup} label={expandedNavbar ? "Groups": ""} />
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={`/organization/${orgId}/support`} IconComponent={HiMiniUserGroup} label={expandedNavbar ? "Support": ""} />
			<NavItem className={expandedNavbar ? "expanded" : "min"} to={'/organizations'} IconComponent={LuLogOut} label={expandedNavbar ? "Organizations": ""} />
		</nav>
	);
};

export default Navbar;