import { useRef } from 'react';
import { IconCloudUpload, IconDownload, IconX } from '@tabler/icons-react';
import { Button, Group, Text, useMantineTheme } from '@mantine/core';
import {Dropzone, MIME_TYPES} from '@mantine/dropzone';
import classes from './DropzoneButton.module.css';

interface DropzoneButtonProps {
    onDrop: (file: File) => void;
}

export function DropzoneButton(props : DropzoneButtonProps) {
    const theme = useMantineTheme();
    const openRef = useRef<() => void>(null);
//    const [files, setFiles] = useState<FileWithPath[]>([]);

    return (
        <div className={classes.wrapper}>
            <Dropzone
                openRef={openRef}
                onDrop={(e) => {
 //                   setFiles(e);
                    props.onDrop(e[e.length-1])
                }}
                className={classes.dropzone}
                radius="md"
                accept={['image/*']}
                maxSize={10 * 1024 ** 2}
            >
                <div style={{ pointerEvents: 'none' }}>
                    <Group justify="center">
                        <Dropzone.Accept>
                            <IconDownload size={50} color={theme.colors.blue[6]} stroke={1.5} />
                        </Dropzone.Accept>
                        <Dropzone.Reject>
                            <IconX size={50} color={theme.colors.red[6]} stroke={1.5} />
                        </Dropzone.Reject>
                        <Dropzone.Idle>
                            <IconCloudUpload size={50} stroke={1.5} className={classes.icon} />
                        </Dropzone.Idle>
                    </Group>

                    <Text ta="center" fw={700} fz="lg" mt="xl">
                        <Dropzone.Accept>Drop files here</Dropzone.Accept>
                        <Dropzone.Reject>File must be an Image and less than 10Mb</Dropzone.Reject>
                        <Dropzone.Idle>Upload product images here</Dropzone.Idle>
                    </Text>

                    <Text className={classes.description}>
                        Drop your picture here to upload.
                    </Text>
                </div>
            </Dropzone>

            <Button className={classes.control} size="md" radius="xl" onClick={() => openRef.current?.()}>
                Select files
            </Button>
        </div>
    );
}