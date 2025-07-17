import React, { useRef } from "react";
import "./Simple.css"

const Simple = ({ schedule }) => {
    const fixedColumnRef = useRef(null);
    const scrollableTableRef = useRef(null);

    const shifts = schedule?.shifts ?? [];

    const uniqueEmployees = [...new Set(
        shifts.flatMap(shift => shift.employees.map(e => e.name))
    )];

    return (
        <>
            <div className="fixed-column" ref={fixedColumnRef}>
                <table>
                    <thead>
                    <tr>
                        <th>Employee</th>
                    </tr>
                    </thead>
                    <tbody>
                    {uniqueEmployees.map(name => (
                        <tr key={name}>
                            <td>{name}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

            <div className="scrollable-table" ref={scrollableTableRef}>
                <table>
                    <thead>
                    <tr>
                        {shifts.map(shift => (
                            <th key={shift.id}>{shift.date}</th>
                        ))}
                    </tr>
                    </thead>
                    <tbody>
                    {uniqueEmployees.map(name => (
                        <tr key={name}>
                            {shifts.map(shift => {
                                const emp = shift.employees.find(e => e.name === name);
                                return (
                                    <td key={shift.id}>
                                        {emp
                                            ? `${shift.startTime} - ${shift.endTime}`
                                            : "—"}
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
};

export default Simple;
