

function ShiftBox({ shift }) {

    const getStartRow = (startTime) => {
        const hours = parseInt(startTime.split(":")[0]);
        return hours + 2;
    };

    const getEndRow = (endTime) => {
        const hours = parseInt(endTime.split(":")[0]);
        if (hours === 0) {
            return 26;
        }
        return hours + 2;
    };

    return (
        <div
            className="shift-box"
            style={{
                backgroundColor: shift.color,
                gridRow: `${getStartRow(shift.from)} / ${getEndRow(shift.to)}`,
                position: "relative",
            }}
        >

            <b>{shift.shiftType}</b>
            <div>{shift.from} - {shift.to}</div>

            <div className='shift-box-employees'>
                {shift.employees.map((emp) => (
                    <div key={emp.id} className={"shift-box-employee"}>
                        <p style={{margin: 0}}>{emp.name}</p>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default ShiftBox;