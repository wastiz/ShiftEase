import {ForwardRefExoticComponent, RefAttributes} from "react";
import {LucideProps} from "lucide-react";

export interface StepData {
    title: string;
    description: string;
    icon: ForwardRefExoticComponent<Omit<LucideProps, "ref"> & RefAttributes<SVGSVGElement>>;
    actionLabel: string;
}