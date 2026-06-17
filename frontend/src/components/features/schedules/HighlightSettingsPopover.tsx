import { Settings } from "lucide-react";
import {Popover, PopoverContent, PopoverTrigger} from "@/components/ui/shadcn/popover";
import {Button} from "@/components/ui/shadcn/button";
import {Checkbox} from "@/components/ui/shadcn/checkbox";
import {Label} from "@/components/ui/shadcn/label";

type Props = {
    showWeekendHighlight: boolean;
    showHolidayHighlight: boolean;
    showShortenedHighlight: boolean;

    onToggleWeekend: (value: boolean) => void;
    onToggleHoliday: (value: boolean) => void;
    onToggleShortened: (value: boolean) => void;
};

export function HighlightSettingsPopover({
                                             showWeekendHighlight,
                                             showHolidayHighlight,
                                             showShortenedHighlight,
                                             onToggleWeekend,
                                             onToggleHoliday,
                                             onToggleShortened,
                                         }: Props) {
    return (
        <Popover>
            <PopoverTrigger asChild>
                <Button variant="outline" size="icon">
                    <Settings className="h-4 w-4" />
                </Button>
            </PopoverTrigger>

            <PopoverContent className="w-64">
                <div className="space-y-4">
                    <h4 className="font-medium text-sm">Highlights</h4>

                    <div className="space-y-3">
                        {/* Weekend */}
                        <div className="flex items-center space-x-2">
                            <Checkbox
                                id="highlight-weekend"
                                checked={showWeekendHighlight}
                                onCheckedChange={(c) => onToggleWeekend(!!c)}
                            />
                            <Label
                                htmlFor="highlight-weekend"
                                className="flex items-center gap-2 cursor-pointer text-sm font-normal"
                            >
                                <div className="w-3 h-3 rounded ring-1 ring-inset ring-yellow-300/60 bg-yellow-50/30" />
                                Weekends
                            </Label>
                        </div>

                        {/* Holiday */}
                        <div className="flex items-center space-x-2">
                            <Checkbox
                                id="highlight-holiday"
                                checked={showHolidayHighlight}
                                onCheckedChange={(c) => onToggleHoliday(!!c)}
                            />
                            <Label
                                htmlFor="highlight-holiday"
                                className="flex items-center gap-2 cursor-pointer text-sm font-normal"
                            >
                                <div className="w-3 h-3 rounded bg-red-100" />
                                Holidays / Day Off
                            </Label>
                        </div>

                        {/* Shortened */}
                        <div className="flex items-center space-x-2">
                            <Checkbox
                                id="highlight-shortened"
                                checked={showShortenedHighlight}
                                onCheckedChange={(c) => onToggleShortened(!!c)}
                            />
                            <Label
                                htmlFor="highlight-shortened"
                                className="flex items-center gap-2 cursor-pointer text-sm font-normal"
                            >
                                <div className="w-3 h-3 rounded bg-orange-100" />
                                Shortened Days
                            </Label>
                        </div>
                    </div>
                </div>
            </PopoverContent>
        </Popover>
    );
}