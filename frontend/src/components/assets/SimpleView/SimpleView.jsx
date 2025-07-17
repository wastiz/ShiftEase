import './SimpleView.css';
import { useEffect, useRef, useMemo } from 'react';

function SimpleView({ shiftData }) {
    const fixedColumnRef = useRef(null);
    const scrollableTableRef = useRef(null);

    const transformedData = useMemo(() => {
        if (!shiftData || !shiftData.shifts) return { employees: [], dates: [] };

        const employeesMap = new Map();
        const dateSet = new Set();

        shiftData.shifts.forEach(shift => {
            shift.employees.forEach(employee => {
                if (!employeesMap.has(employee.id)) {
                    employeesMap.set(employee.id, {
                        employeeId: employee.id,
                        employeeName: `${employee.name}`,
                        shifts: {}
                    });
                }
                employeesMap.get(employee.id).shifts[shift.date] = {
                    startTime: shift.from,
                    endTime: shift.to,
                    shiftType: shift.shiftType
                };
            });

            dateSet.add(shift.date);
        });

        return {
            employees: Array.from(employeesMap.values()),
            dates: Array.from(dateSet).sort()
        };
    }, [shiftData]);

    useEffect(() => {
        if (!fixedColumnRef.current || !scrollableTableRef.current) return;
        const fixedCells = fixedColumnRef.current.querySelectorAll('td, th');
        const scrollableCells = scrollableTableRef.current.querySelectorAll('td, th');

        fixedCells.forEach((cell, index) => {
            const scrollableCell = scrollableCells[index];
            if (scrollableCell) {
                scrollableCell.style.width = `${cell.offsetWidth}px`;
            }
        });
    }, [transformedData]);

    return (
        <>
            <div className='fixed-column' ref={fixedColumnRef}>
                <table>
                    <thead>
                        <tr>
                            <th>Employee</th>
                        </tr>
                    </thead>
                    <tbody>
                        {transformedData.employees.map(employee => (
                            <tr key={employee.employeeId}>
                                <td>{employee.employeeName}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div className='scrollable-table' ref={scrollableTableRef}>
                <table>
                    <thead>
                        <tr>
                            {transformedData.dates.map(date => (
                                <th key={date}>{date}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {transformedData.employees.map(employee => (
                            <tr key={employee.employeeId}>
                                {transformedData.dates.map(date => {
                                    const shift = employee.shifts[date];
                                    return (
                                        <td key={date} className={shift ? '' : 'empty-cell'}>
                                            {shift ? `${shift.startTime} - ${shift.endTime}` : "—"}
                                        </td>
                                    );
                                })}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </>
    );
}

export default SimpleView;
