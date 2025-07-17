import { useNavigate } from "react-router-dom";

const EntityCheckResult = ({ entities }) => {
    const navigate = useNavigate();

    const entityMessages = {
        groups: {
            message: 'You have no group, please, create one',
            button: 'Create Groups',
            path: '/add-group'
        },
        employees: {
            message: 'No employees. Please, create one',
            button: 'Create Employees',
            path: '/add-employees'
        },
        shiftTypes: {
            message: 'No Shift Types. Please, create one',
            button: 'Create Shift Types',
            path: '/shift-types'
        },
        schedules: {
            message: 'You have no schedules. Create One',
            button: 'Create Schedule',
            path: '/create-schedule'
        }
    };

    const missingEntities = Object.entries(entities)
        .filter(([key, value]) => !value)
        .map(([key]) => key);

    if (missingEntities.length === 0) {
        return <div>All entities are created</div>;
    }

    return (
        <div className="entity-alerts">
            <h3>Before creating schedule you need:</h3>
            <ul>
                {missingEntities.map(entity => (
                    <li key={entity} className="alert-item">
                        <div>
                            <span>{entityMessages[entity].message}</span>
                            <button
                                onClick={() => navigate(entityMessages[entity].path)}
                                className="action-button"
                            >
                                {entityMessages[entity].button}
                            </button>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default EntityCheckResult;